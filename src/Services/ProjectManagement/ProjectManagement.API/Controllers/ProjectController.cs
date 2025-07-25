// © 2024 DecVCPlat. All rights reserved.

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectManagement.API.Data;
using ProjectManagement.API.Models.DTOs;
using ProjectManagement.API.Models.Entities;
using System.Security.Claims;

namespace ProjectManagement.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
[Authorize]
public class ProjectController : ControllerBase
{
    private readonly ProjectDbContext _context;
    private readonly ILogger<ProjectController> _logger;

    public ProjectController(ProjectDbContext context, ILogger<ProjectController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Create a new project proposal for DecVCPlat
    /// </summary>
    /// <param name="request">Project creation details</param>
    /// <returns>Created project information</returns>
    [HttpPost]
    [Authorize(Policy = "DecVCPlat-Founder")]
    [ProducesResponseType(typeof(ProjectResponse), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public async Task<ActionResult<ProjectResponse>> CreateProject([FromBody] CreateProjectRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var userName = GetCurrentUserName();

            if (!userId.HasValue)
            {
                return Unauthorized("User ID not found in token");
            }

            // Validate milestone funding amounts don't exceed funding goal
            var totalMilestoneFunding = request.Milestones.Sum(m => m.FundingAmount);
            if (totalMilestoneFunding > request.FundingGoal)
            {
                return BadRequest("Total milestone funding cannot exceed project funding goal");
            }

            // Create the project
            var project = new Project
            {
                Id = Guid.NewGuid(),
                Title = request.Title,
                Description = request.Description,
                FounderId = userId.Value,
                FounderName = userName ?? "Unknown",
                Category = request.Category,
                FundingGoal = request.FundingGoal,
                ImageUrl = request.ImageUrl,
                VideoUrl = request.VideoUrl,
                WebsiteUrl = request.WebsiteUrl,
                GitHubUrl = request.GitHubUrl,
                WhitepaperUrl = request.WhitepaperUrl,
                RiskLevel = request.RiskLevel,
                RiskAssessment = request.RiskAssessment,
                FundingDeadline = request.FundingDeadline,
                Status = ProjectStatus.Draft,
                CreatedAt = DateTime.UtcNow
            };

            // Add milestones
            foreach (var milestoneReq in request.Milestones)
            {
                var milestone = new ProjectMilestone
                {
                    Id = Guid.NewGuid(),
                    ProjectId = project.Id,
                    Title = milestoneReq.Title,
                    Description = milestoneReq.Description,
                    FundingAmount = milestoneReq.FundingAmount,
                    DueDate = milestoneReq.DueDate,
                    Status = MilestoneStatus.Pending,
                    CreatedAt = DateTime.UtcNow
                };
                project.Milestones.Add(milestone);
            }

            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

            _logger.LogInformation("DecVCPlat project created successfully: {ProjectId} by founder {FounderId}", 
                project.Id, userId.Value);

            return CreatedAtAction(nameof(GetProject), new { id = project.Id }, MapToProjectResponse(project));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating DecVCPlat project for founder {UserId}", GetCurrentUserId());
            return BadRequest("Failed to create project");
        }
    }

    /// <summary>
    /// Get project details by ID
    /// </summary>
    /// <param name="id">Project ID</param>
    /// <returns>Project details</returns>
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ProjectResponse), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<ProjectResponse>> GetProject(Guid id)
    {
        try
        {
            var project = await _context.Projects
                .Include(p => p.Milestones)
                .Include(p => p.Votes)
                .Include(p => p.Documents.Where(d => d.IsPublic))
                .Include(p => p.Comments.Where(c => !c.IsDeleted))
                    .ThenInclude(c => c.Replies.Where(r => !r.IsDeleted))
                .Include(p => p.FundingRounds.Where(f => f.Status == FundingStatus.Confirmed))
                .FirstOrDefaultAsync(p => p.Id == id);

            if (project == null)
            {
                return NotFound($"Project with ID {id} not found");
            }

            return Ok(MapToProjectResponse(project));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving DecVCPlat project {ProjectId}", id);
            return BadRequest("Failed to retrieve project");
        }
    }

    /// <summary>
    /// Get all projects with pagination and filtering
    /// </summary>
    /// <param name="category">Filter by category</param>
    /// <param name="status">Filter by status</param>
    /// <param name="page">Page number (1-based)</param>
    /// <param name="pageSize">Items per page</param>
    /// <returns>Paginated list of projects</returns>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PaginatedResult<ProjectResponse>), 200)]
    public async Task<ActionResult<PaginatedResult<ProjectResponse>>> GetProjects(
        [FromQuery] string? category = null,
        [FromQuery] ProjectStatus? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var query = _context.Projects.AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(p => p.Category.ToLower() == category.ToLower());
            }

            if (status.HasValue)
            {
                query = query.Where(p => p.Status == status.Value);
            }

            // Get total count for pagination
            var totalCount = await query.CountAsync();

            // Apply pagination
            var projects = await query
                .Include(p => p.Milestones.Take(3)) // Limit milestones for list view
                .Include(p => p.Votes)
                .Include(p => p.FundingRounds.Where(f => f.Status == FundingStatus.Confirmed))
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var projectResponses = projects.Select(MapToProjectResponse).ToList();

            var result = new PaginatedResult<ProjectResponse>
            {
                Items = projectResponses,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving DecVCPlat projects list");
            return BadRequest("Failed to retrieve projects");
        }
    }

    /// <summary>
    /// Submit project for review by luminaries
    /// </summary>
    /// <param name="id">Project ID</param>
    /// <returns>Success response</returns>
    [HttpPost("{id:guid}/submit")]
    [Authorize(Policy = "DecVCPlat-Founder")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> SubmitProject(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == id);

            if (project == null)
            {
                return NotFound($"Project with ID {id} not found");
            }

            if (project.FounderId != userId)
            {
                return Forbid("You can only submit your own projects");
            }

            if (project.Status != ProjectStatus.Draft)
            {
                return BadRequest("Only draft projects can be submitted for review");
            }

            project.Status = ProjectStatus.Submitted;
            project.SubmittedAt = DateTime.UtcNow;
            project.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("DecVCPlat project submitted for review: {ProjectId}", id);
            return Ok(new { message = "Project submitted successfully for luminary review" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting DecVCPlat project {ProjectId}", id);
            return BadRequest("Failed to submit project");
        }
    }

    /// <summary>
    /// Vote on a project (investors only)
    /// </summary>
    /// <param name="id">Project ID</param>
    /// <param name="request">Vote details</param>
    /// <returns>Success response</returns>
    [HttpPost("{id:guid}/vote")]
    [Authorize(Policy = "DecVCPlat-Investor")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> VoteOnProject(Guid id, [FromBody] VoteRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var userName = GetCurrentUserName();

            if (!userId.HasValue)
            {
                return Unauthorized("User ID not found in token");
            }

            var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == id);
            if (project == null)
            {
                return NotFound($"Project with ID {id} not found");
            }

            if (project.Status != ProjectStatus.Approved)
            {
                return BadRequest("You can only vote on approved projects");
            }

            // Check if user has already voted
            var existingVote = await _context.ProjectVotes
                .FirstOrDefaultAsync(v => v.ProjectId == id && v.VoterId == userId.Value);

            if (existingVote != null)
            {
                return BadRequest("You have already voted on this project");
            }

            var vote = new ProjectVote
            {
                Id = Guid.NewGuid(),
                ProjectId = id,
                VoterId = userId.Value,
                VoterName = userName ?? "Unknown",
                VoteType = request.VoteType,
                StakedTokens = request.StakedTokens,
                VotingPower = request.StakedTokens, // 1:1 for simplicity
                Comments = request.Comments,
                VotedAt = DateTime.UtcNow
            };

            _context.ProjectVotes.Add(vote);

            // Update project voting statistics
            project.TotalVotes++;
            if (request.VoteType == VoteType.Support)
            {
                project.VotingPower += (int)request.StakedTokens;
            }
            else if (request.VoteType == VoteType.Against)
            {
                project.VotingPower -= (int)request.StakedTokens;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Vote cast on DecVCPlat project {ProjectId} by investor {InvestorId}", id, userId.Value);
            return Ok(new { message = "Vote cast successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error casting vote on DecVCPlat project {ProjectId}", id);
            return BadRequest("Failed to cast vote");
        }
    }

    private Guid? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("user_id")?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
    }

    private string? GetCurrentUserName()
    {
        return User.FindFirst(ClaimTypes.Name)?.Value ?? User.FindFirst("full_name")?.Value;
    }

    private static ProjectResponse MapToProjectResponse(Project project)
    {
        return new ProjectResponse
        {
            Id = project.Id,
            Title = project.Title,
            Description = project.Description,
            FounderId = project.FounderId,
            FounderName = project.FounderName,
            Category = project.Category,
            FundingGoal = project.FundingGoal,
            CurrentFunding = project.CurrentFunding,
            VotingPower = project.VotingPower,
            TotalVotes = project.TotalVotes,
            Status = project.Status,
            ImageUrl = project.ImageUrl,
            VideoUrl = project.VideoUrl,
            WebsiteUrl = project.WebsiteUrl,
            GitHubUrl = project.GitHubUrl,
            WhitepaperUrl = project.WhitepaperUrl,
            CreatedAt = project.CreatedAt,
            UpdatedAt = project.UpdatedAt,
            SubmittedAt = project.SubmittedAt,
            ApprovedAt = project.ApprovedAt,
            FundingDeadline = project.FundingDeadline,
            RiskLevel = project.RiskLevel,
            RiskAssessment = project.RiskAssessment,
            SmartContractAddress = project.SmartContractAddress,
            IsSmartContractDeployed = project.IsSmartContractDeployed,
            Milestones = project.Milestones.Select(m => new MilestoneResponse
            {
                Id = m.Id,
                ProjectId = m.ProjectId,
                Title = m.Title,
                Description = m.Description,
                FundingAmount = m.FundingAmount,
                DueDate = m.DueDate,
                Status = m.Status,
                CreatedAt = m.CreatedAt,
                CompletedAt = m.CompletedAt,
                ApprovedAt = m.ApprovedAt,
                ApprovedByUserId = m.ApprovedByUserId,
                CompletionNotes = m.CompletionNotes,
                ProofDocumentUrl = m.ProofDocumentUrl,
                BlockchainTransactionHash = m.BlockchainTransactionHash,
                IsFundingReleased = m.IsFundingReleased
            }).ToList(),
            Votes = project.Votes.Select(v => new VoteResponse
            {
                Id = v.Id,
                ProjectId = v.ProjectId,
                VoterId = v.VoterId,
                VoterName = v.VoterName,
                VoteType = v.VoteType,
                StakedTokens = v.StakedTokens,
                VotingPower = v.VotingPower,
                Comments = v.Comments,
                VotedAt = v.VotedAt,
                BlockchainTransactionHash = v.BlockchainTransactionHash,
                IsVerifiedOnChain = v.IsVerifiedOnChain
            }).ToList(),
            Documents = project.Documents.Select(d => new DocumentResponse
            {
                Id = d.Id,
                ProjectId = d.ProjectId,
                FileName = d.FileName,
                FilePath = d.FilePath,
                ContentType = d.ContentType,
                FileSize = d.FileSize,
                DocumentType = d.DocumentType,
                Description = d.Description,
                UploadedAt = d.UploadedAt,
                UploadedByUserId = d.UploadedByUserId,
                UploadedByUserName = d.UploadedByUserName,
                IsPublic = d.IsPublic
            }).ToList(),
            Comments = project.Comments.Select(c => new CommentResponse
            {
                Id = c.Id,
                ProjectId = c.ProjectId,
                AuthorId = c.AuthorId,
                AuthorName = c.AuthorName,
                Content = c.Content,
                CommentType = c.CommentType,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt,
                IsEdited = c.IsEdited,
                ParentCommentId = c.ParentCommentId,
                Replies = c.Replies.Select(r => new CommentResponse
                {
                    Id = r.Id,
                    ProjectId = r.ProjectId,
                    AuthorId = r.AuthorId,
                    AuthorName = r.AuthorName,
                    Content = r.Content,
                    CommentType = r.CommentType,
                    CreatedAt = r.CreatedAt,
                    UpdatedAt = r.UpdatedAt,
                    IsEdited = r.IsEdited,
                    ParentCommentId = r.ParentCommentId
                }).ToList()
            }).ToList(),
            FundingRounds = project.FundingRounds.Select(f => new FundingResponse
            {
                Id = f.Id,
                ProjectId = f.ProjectId,
                InvestorId = f.InvestorId,
                InvestorName = f.InvestorName,
                Amount = f.Amount,
                FundingType = f.FundingType,
                Status = f.Status,
                FundedAt = f.FundedAt,
                ProcessedAt = f.ProcessedAt,
                TransactionHash = f.TransactionHash,
                WalletAddress = f.WalletAddress,
                IsVerifiedOnChain = f.IsVerifiedOnChain,
                MilestoneId = f.MilestoneId,
                Notes = f.Notes,
                StakedTokens = f.StakedTokens,
                VotingPowerGranted = f.VotingPowerGranted
            }).ToList()
        };
    }
}

public class VoteRequest
{
    public VoteType VoteType { get; set; }
    public decimal StakedTokens { get; set; }
    public string? Comments { get; set; }
}

public class PaginatedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}
