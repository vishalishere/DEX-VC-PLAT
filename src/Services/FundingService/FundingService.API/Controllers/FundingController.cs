// © 2024 DecVCPlat. All rights reserved.

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FundingService.API.Data;
using FundingService.API.Models.DTOs;
using FundingService.API.Models.Entities;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace FundingService.API.Controllers;

/// <summary>
/// Controller for managing funding operations in the DecVCPlat platform
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
[Authorize]
public class FundingController : ControllerBase
{
    private readonly FundingDbContext _context;
    private readonly ILogger<FundingController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="FundingController"/> class
    /// </summary>
    /// <param name="context">The funding database context</param>
    /// <param name="logger">The logger</param>
    public FundingController(FundingDbContext context, ILogger<FundingController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new funding tranche for a project milestone
    /// </summary>
    /// <param name="request">The tranche creation request</param>
    /// <returns>The created tranche details</returns>
    /// <response code="201">Returns the created tranche</response>
    /// <response code="400">If the request is invalid or tranche already exists</response>
    /// <response code="401">If the user is not authenticated</response>
    /// <response code="403">If the user is not authorized</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpPost("tranches")]
    [Authorize(Policy = "DecVCPlat-Luminary")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<TrancheResponse>> CreateTranche([FromBody] CreateTrancheRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue) return Unauthorized("User ID not found");

            // Check if tranche already exists for this milestone
            var existingTranche = await _context.FundingTranches
                .FirstOrDefaultAsync(t => t.ProjectId == request.ProjectId && t.MilestoneId == request.MilestoneId);
            if (existingTranche != null)
                return BadRequest("Funding tranche already exists for this milestone");

            var tranche = new FundingTranche
            {
                Id = Guid.NewGuid(),
                ProjectId = request.ProjectId,
                MilestoneId = request.MilestoneId,
                Title = request.Title,
                Description = request.Description,
                Amount = request.Amount,
                TrancheNumber = request.TrancheNumber,
                ScheduledReleaseDate = request.ScheduledReleaseDate,
                MinVotingThreshold = request.MinVotingThreshold,
                RequiresBoardApproval = request.RequiresBoardApproval,
                RequiresMilestoneEvidence = request.RequiresMilestoneEvidence,
                RecipientWalletAddress = request.RecipientWalletAddress,
                Status = TrancheStatus.Pending,
                IsInEscrow = true,
                EscrowCreatedAt = DateTime.UtcNow
            };

            _context.FundingTranches.Add(tranche);
            await _context.SaveChangesAsync();

            _logger.LogInformation("DecVCPlat funding tranche created: {TrancheId} for project {ProjectId}", 
                tranche.Id, request.ProjectId);

            return CreatedAtAction(nameof(GetTranche), new { id = tranche.Id }, MapToTrancheResponse(tranche));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating DecVCPlat funding tranche");
            return BadRequest("Failed to create funding tranche");
        }
    }

    [HttpGet("tranches/{id:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<TrancheResponse>> GetTranche(Guid id)
    {
        try
        {
            var tranche = await _context.FundingTranches
                .Include(t => t.Releases)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tranche == null)
                return NotFound($"Funding tranche {id} not found");

            return Ok(MapToTrancheResponse(tranche));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving DecVCPlat funding tranche {TrancheId}", id);
            return BadRequest("Failed to retrieve funding tranche");
        }
    }

    [HttpGet("tranches/project/{projectId:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<List<TrancheResponse>>> GetProjectTranches(Guid projectId)
    {
        try
        {
            var tranches = await _context.FundingTranches
                .Include(t => t.Releases)
                .Where(t => t.ProjectId == projectId)
                .OrderBy(t => t.TrancheNumber)
                .ToListAsync();

            var responses = tranches.Select(MapToTrancheResponse).ToList();
            return Ok(responses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving DecVCPlat funding tranches for project {ProjectId}", projectId);
            return BadRequest("Failed to retrieve project tranches");
        }
    }

    [HttpPost("tranches/{id:guid}/approve")]
    [Authorize(Policy = "DecVCPlat-Luminary")]
    public async Task<IActionResult> ApproveFunding(Guid id, [FromBody] ApproveFundsRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var userName = GetCurrentUserName();
            if (!userId.HasValue) return Unauthorized("User ID not found");

            var tranche = await _context.FundingTranches.FindAsync(id);
            if (tranche == null) return NotFound("Funding tranche not found");

            if (tranche.Status != TrancheStatus.AwaitingApproval)
                return BadRequest("Tranche is not awaiting approval");

            tranche.IsLuminaryApproved = request.IsApproved;
            tranche.ApprovedByLuminaryId = userId.Value;
            tranche.ApprovedAt = DateTime.UtcNow;
            tranche.MilestoneEvidenceUrl = request.MilestoneEvidenceUrl;
            tranche.ReleaseNotes = request.ApprovalNotes;
            tranche.Status = request.IsApproved ? TrancheStatus.Approved : TrancheStatus.Cancelled;
            tranche.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("DecVCPlat funding tranche {TrancheId} {Action} by luminary {UserId}", 
                id, request.IsApproved ? "approved" : "rejected", userId.Value);

            return Ok(new { 
                message = request.IsApproved ? "Funding approved" : "Funding rejected", 
                status = tranche.Status 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving DecVCPlat funding tranche {TrancheId}", id);
            return BadRequest("Failed to process approval");
        }
    }

    [HttpPost("tranches/{id:guid}/release")]
    [Authorize(Policy = "DecVCPlat-Luminary")]
    public async Task<IActionResult> ReleaseFunds(Guid id, [FromBody] ReleaseFundsRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var userName = GetCurrentUserName();
            if (!userId.HasValue) return Unauthorized("User ID not found");

            var tranche = await _context.FundingTranches.FindAsync(id);
            if (tranche == null) return NotFound("Funding tranche not found");

            if (tranche.Status != TrancheStatus.Approved)
                return BadRequest("Tranche must be approved before funds can be released");

            if (!tranche.IsLuminaryApproved)
                return BadRequest("Tranche requires luminary approval");

            if (request.Amount > tranche.Amount - tranche.ActualAmountReleased)
                return BadRequest("Release amount exceeds remaining tranche balance");

            var release = new FundingRelease
            {
                Id = Guid.NewGuid(),
                TrancheId = id,
                ProjectId = tranche.ProjectId,
                Amount = request.Amount,
                PaymentMethod = request.PaymentMethod,
                ToWalletAddress = request.RecipientWalletAddress,
                ProcessedByUserId = userId.Value,
                ProcessedByUserName = userName ?? "Unknown",
                ProcessingNotes = request.ProcessingNotes,
                RequiresManualReview = request.RequiresManualReview,
                Status = request.RequiresManualReview ? ReleaseStatus.UnderReview : ReleaseStatus.Processing
            };

            // Simulate blockchain transaction for demo
            if (request.PaymentMethod == PaymentMethod.Blockchain)
            {
                release.FromWalletAddress = tranche.EscrowWalletAddress ?? "0xDecVCPlatEscrow123456789";
                release.TransactionHash = $"0x{Guid.NewGuid():N}";
                release.BlockNumber = new Random().Next(10000000, 99999999);
                release.GasFee = 0.001m;
                release.IsVerified = true;
                release.VerifiedAt = DateTime.UtcNow;
                release.VerifiedByUserId = userId.Value;
                release.VerifiedByUserName = userName;
            }

            // Complete release if not under review
            if (!request.RequiresManualReview)
            {
                release.Status = ReleaseStatus.Completed;
                release.CompletedAt = DateTime.UtcNow;
                tranche.ActualAmountReleased += request.Amount;
                tranche.ActualReleaseDate = DateTime.UtcNow;
                tranche.Status = (tranche.ActualAmountReleased >= tranche.Amount) ? TrancheStatus.Released : TrancheStatus.Approved;
                tranche.ReleaseTransactionHash = release.TransactionHash;
                tranche.IsReleasedOnChain = true;
            }

            _context.FundingReleases.Add(release);
            tranche.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("DecVCPlat funding released: {Amount} from tranche {TrancheId} to {Recipient}", 
                request.Amount, id, request.RecipientWalletAddress);

            return Ok(new { 
                message = "Funds release initiated", 
                releaseId = release.Id,
                status = release.Status,
                transactionHash = release.TransactionHash 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error releasing DecVCPlat funds from tranche {TrancheId}", id);
            return BadRequest("Failed to release funds");
        }
    }

    [HttpPost("tranches/{id:guid}/milestone-complete")]
    [Authorize(Policy = "DecVCPlat-Founder")]
    public async Task<IActionResult> MarkMilestoneComplete(Guid id, [FromBody] string evidenceUrl)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue) return Unauthorized("User ID not found");

            var tranche = await _context.FundingTranches.FindAsync(id);
            if (tranche == null) return NotFound("Funding tranche not found");

            if (tranche.Status != TrancheStatus.Pending && tranche.Status != TrancheStatus.InEscrow)
                return BadRequest("Tranche is not in a state where milestone can be marked complete");

            tranche.IsMilestoneCompleted = true;
            tranche.MilestoneEvidenceUrl = evidenceUrl;
            tranche.Status = TrancheStatus.AwaitingApproval;
            tranche.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("DecVCPlat milestone marked complete for tranche {TrancheId} by founder {UserId}", 
                id, userId.Value);

            return Ok(new { message = "Milestone marked as complete, awaiting luminary approval" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking milestone complete for DecVCPlat tranche {TrancheId}", id);
            return BadRequest("Failed to mark milestone complete");
        }
    }

    [HttpGet("releases/{id:guid}")]
    [Authorize]
    public async Task<ActionResult<ReleaseResponse>> GetRelease(Guid id)
    {
        try
        {
            var release = await _context.FundingReleases
                .Include(r => r.Tranche)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (release == null)
                return NotFound($"Funding release {id} not found");

            return Ok(MapToReleaseResponse(release));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving DecVCPlat funding release {ReleaseId}", id);
            return BadRequest("Failed to retrieve funding release");
        }
    }

    [HttpGet("analytics/project/{projectId:guid}")]
    [Authorize]
    public async Task<ActionResult<ProjectFundingAnalytics>> GetProjectFundingAnalytics(Guid projectId)
    {
        try
        {
            var tranches = await _context.FundingTranches
                .Include(t => t.Releases)
                .Where(t => t.ProjectId == projectId)
                .ToListAsync();

            var analytics = new ProjectFundingAnalytics
            {
                ProjectId = projectId,
                TotalTranches = tranches.Count,
                TotalPlannedAmount = tranches.Sum(t => t.Amount),
                TotalReleasedAmount = tranches.Sum(t => t.ActualAmountReleased),
                CompletedTranches = tranches.Count(t => t.Status == TrancheStatus.Released),
                PendingTranches = tranches.Count(t => t.Status == TrancheStatus.Pending || t.Status == TrancheStatus.InEscrow),
                ApprovedTranches = tranches.Count(t => t.Status == TrancheStatus.Approved),
                TotalReleases = tranches.SelectMany(t => t.Releases).Count(),
                AverageReleaseTime = CalculateAverageReleaseTime(tranches)
            };

            return Ok(analytics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving DecVCPlat funding analytics for project {ProjectId}", projectId);
            return BadRequest("Failed to retrieve funding analytics");
        }
    }

    private static TrancheResponse MapToTrancheResponse(FundingTranche tranche)
    {
        return new TrancheResponse
        {
            Id = tranche.Id,
            ProjectId = tranche.ProjectId,
            MilestoneId = tranche.MilestoneId,
            Title = tranche.Title,
            Description = tranche.Description,
            Amount = tranche.Amount,
            TrancheNumber = tranche.TrancheNumber,
            Status = tranche.Status,
            ScheduledReleaseDate = tranche.ScheduledReleaseDate,
            ActualReleaseDate = tranche.ActualReleaseDate,
            IsMilestoneCompleted = tranche.IsMilestoneCompleted,
            IsLuminaryApproved = tranche.IsLuminaryApproved,
            ApprovedByLuminaryId = tranche.ApprovedByLuminaryId,
            ApprovedAt = tranche.ApprovedAt,
            SmartContractAddress = tranche.SmartContractAddress,
            ReleaseTransactionHash = tranche.ReleaseTransactionHash,
            IsReleasedOnChain = tranche.IsReleasedOnChain,
            EscrowWalletAddress = tranche.EscrowWalletAddress,
            IsInEscrow = tranche.IsInEscrow,
            MinVotingThreshold = tranche.MinVotingThreshold,
            RequiresBoardApproval = tranche.RequiresBoardApproval,
            RequiresMilestoneEvidence = tranche.RequiresMilestoneEvidence,
            MilestoneEvidenceUrl = tranche.MilestoneEvidenceUrl,
            ProcessingFee = tranche.ProcessingFee,
            ActualAmountReleased = tranche.ActualAmountReleased,
            RecipientWalletAddress = tranche.RecipientWalletAddress,
            CreatedAt = tranche.CreatedAt,
            UpdatedAt = tranche.UpdatedAt,
            ReleaseNotes = tranche.ReleaseNotes,
            Releases = tranche.Releases.Select(MapToReleaseResponse).ToList()
        };
    }

    private static ReleaseResponse MapToReleaseResponse(FundingRelease release)
    {
        return new ReleaseResponse
        {
            Id = release.Id,
            TrancheId = release.TrancheId,
            ProjectId = release.ProjectId,
            Amount = release.Amount,
            Status = release.Status,
            InitiatedAt = release.InitiatedAt,
            CompletedAt = release.CompletedAt,
            PaymentMethod = release.PaymentMethod,
            FromWalletAddress = release.FromWalletAddress,
            ToWalletAddress = release.ToWalletAddress,
            TransactionHash = release.TransactionHash,
            BlockNumber = release.BlockNumber,
            GasFee = release.GasFee,
            BankTransactionId = release.BankTransactionId,
            BankReference = release.BankReference,
            ProcessedByUserId = release.ProcessedByUserId,
            ProcessedByUserName = release.ProcessedByUserName,
            ProcessingFeeCharged = release.ProcessingFeeCharged,
            ProcessingNotes = release.ProcessingNotes,
            IsVerified = release.IsVerified,
            VerifiedAt = release.VerifiedAt,
            IsHighRisk = release.IsHighRisk,
            RequiresManualReview = release.RequiresManualReview
        };
    }

    private static double CalculateAverageReleaseTime(List<FundingTranche> tranches)
    {
        var completedReleases = tranches
            .SelectMany(t => t.Releases)
            .Where(r => r.CompletedAt.HasValue)
            .ToList();

        if (!completedReleases.Any()) return 0;

        var totalHours = completedReleases
            .Sum(r => (r.CompletedAt!.Value - r.InitiatedAt).TotalHours);

        return totalHours / completedReleases.Count;
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
}

public class ProjectFundingAnalytics
{
    public Guid ProjectId { get; set; }
    public int TotalTranches { get; set; }
    public decimal TotalPlannedAmount { get; set; }
    public decimal TotalReleasedAmount { get; set; }
    public int CompletedTranches { get; set; }
    public int PendingTranches { get; set; }
    public int ApprovedTranches { get; set; }
    public int TotalReleases { get; set; }
    public double AverageReleaseTime { get; set; }
    public decimal FundingProgress => TotalPlannedAmount > 0 ? (TotalReleasedAmount / TotalPlannedAmount) * 100 : 0;
}
