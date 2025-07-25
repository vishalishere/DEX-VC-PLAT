// © 2024 DecVCPlat. All rights reserved.

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VotingService.API.Data;
using VotingService.API.Models.DTOs;
using VotingService.API.Models.Entities;
using System.Security.Claims;

namespace VotingService.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
[Authorize]
public class VotingController : ControllerBase
{
    private readonly VotingDbContext _context;
    private readonly ILogger<VotingController> _logger;

    public VotingController(VotingDbContext context, ILogger<VotingController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpPost("proposals")]
    [Authorize(Policy = "DecVCPlat-Luminary")]
    public async Task<ActionResult<VotingProposal>> CreateProposal([FromBody] CreateProposalRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue) return Unauthorized("User ID not found");

            var proposal = new VotingProposal
            {
                Id = Guid.NewGuid(),
                ProjectId = request.ProjectId,
                Title = request.Title,
                Description = request.Description,
                ProposalType = request.ProposalType,
                StartDate = DateTime.UtcNow,
                EndDate = request.EndDate,
                MinTokensRequired = request.MinTokensRequired,
                QuorumRequired = request.QuorumRequired,
                RequiresLuminaryApproval = request.RequiresLuminaryApproval,
                ApprovedByLuminaryId = userId.Value,
                ApprovedAt = DateTime.UtcNow,
                Status = VotingStatus.Active
            };

            _context.VotingProposals.Add(proposal);
            await _context.SaveChangesAsync();

            _logger.LogInformation("DecVCPlat voting proposal created: {ProposalId} for project {ProjectId}", 
                proposal.Id, request.ProjectId);

            return CreatedAtAction(nameof(GetProposal), new { id = proposal.Id }, proposal);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating DecVCPlat voting proposal");
            return BadRequest("Failed to create proposal");
        }
    }

    [HttpGet("proposals/{id:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<VotingProposal>> GetProposal(Guid id)
    {
        try
        {
            var proposal = await _context.VotingProposals
                .Include(p => p.TokenStakes.Where(s => s.Status == StakeStatus.Active))
                .Include(p => p.Votes)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (proposal == null)
                return NotFound($"Proposal {id} not found");

            return Ok(proposal);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving DecVCPlat voting proposal {ProposalId}", id);
            return BadRequest("Failed to retrieve proposal");
        }
    }

    [HttpGet("proposals")]
    [AllowAnonymous]
    public async Task<ActionResult<List<VotingProposal>>> GetProposals([FromQuery] VotingStatus? status = null)
    {
        try
        {
            var query = _context.VotingProposals.AsQueryable();

            if (status.HasValue)
                query = query.Where(p => p.Status == status.Value);

            var proposals = await query
                .Include(p => p.TokenStakes.Where(s => s.Status == StakeStatus.Active))
                .Include(p => p.Votes)
                .OrderByDescending(p => p.CreatedAt)
                .Take(50)
                .ToListAsync();

            return Ok(proposals);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving DecVCPlat voting proposals");
            return BadRequest("Failed to retrieve proposals");
        }
    }

    [HttpPost("stake")]
    [Authorize(Policy = "DecVCPlat-Investor")]
    public async Task<IActionResult> StakeTokens([FromBody] StakeTokensRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var userName = GetCurrentUserName();
            if (!userId.HasValue) return Unauthorized("User ID not found");

            var proposal = await _context.VotingProposals.FindAsync(request.ProposalId);
            if (proposal == null) return NotFound("Proposal not found");
            if (proposal.Status != VotingStatus.Active) return BadRequest("Proposal is not active");
            if (DateTime.UtcNow > proposal.EndDate) return BadRequest("Proposal has expired");

            var existingStake = await _context.TokenStakes
                .FirstOrDefaultAsync(s => s.ProposalId == request.ProposalId && s.UserId == userId.Value);

            if (existingStake != null)
            {
                existingStake.TokenAmount += request.TokenAmount;
                existingStake.Notes = request.Notes;
            }
            else
            {
                var stake = new TokenStake
                {
                    Id = Guid.NewGuid(),
                    ProposalId = request.ProposalId,
                    UserId = userId.Value,
                    UserName = userName ?? "Unknown",
                    WalletAddress = request.WalletAddress,
                    TokenAmount = request.TokenAmount,
                    Status = StakeStatus.Active,
                    LockedUntil = proposal.EndDate.AddDays(7), // 7-day lock after proposal ends
                    Notes = request.Notes
                };
                _context.TokenStakes.Add(stake);
            }

            proposal.TotalTokensStaked += request.TokenAmount;
            proposal.TotalVoters = await _context.TokenStakes
                .CountAsync(s => s.ProposalId == request.ProposalId && s.Status == StakeStatus.Active);

            await _context.SaveChangesAsync();

            _logger.LogInformation("Tokens staked in DecVCPlat proposal {ProposalId}: {Amount} by user {UserId}", 
                request.ProposalId, request.TokenAmount, userId.Value);

            return Ok(new { message = "Tokens staked successfully", totalStaked = proposal.TotalTokensStaked });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error staking tokens in DecVCPlat proposal {ProposalId}", request.ProposalId);
            return BadRequest("Failed to stake tokens");
        }
    }

    [HttpPost("vote")]
    [Authorize(Policy = "DecVCPlat-Investor")]
    public async Task<IActionResult> CastVote([FromBody] CastVoteRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var userName = GetCurrentUserName();
            if (!userId.HasValue) return Unauthorized("User ID not found");

            var proposal = await _context.VotingProposals.FindAsync(request.ProposalId);
            if (proposal == null) return NotFound("Proposal not found");
            if (proposal.Status != VotingStatus.Active) return BadRequest("Proposal is not active");
            if (DateTime.UtcNow > proposal.EndDate) return BadRequest("Proposal has expired");

            var existingVote = await _context.Votes
                .FirstOrDefaultAsync(v => v.ProposalId == request.ProposalId && v.VoterId == userId.Value);
            if (existingVote != null) return BadRequest("You have already voted on this proposal");

            var userStake = await _context.TokenStakes
                .FirstOrDefaultAsync(s => s.ProposalId == request.ProposalId && s.UserId == userId.Value && s.Status == StakeStatus.Active);
            if (userStake == null) return BadRequest("You must stake tokens before voting");

            var votingPower = userStake.TokenAmount * 1.0m; // Simple 1:1 voting power

            var vote = new Vote
            {
                Id = Guid.NewGuid(),
                ProposalId = request.ProposalId,
                VoterId = userId.Value,
                VoterName = request.IsAnonymous ? "Anonymous" : userName ?? "Unknown",
                Choice = request.Choice,
                VotingPower = votingPower,
                BaseVotingPower = userStake.TokenAmount,
                StakeMultiplier = 1.0m,
                ReputationMultiplier = 1.0m,
                Comments = request.Comments,
                IsAnonymous = request.IsAnonymous
            };

            _context.Votes.Add(vote);

            // Update proposal vote counts
            switch (request.Choice)
            {
                case VoteChoice.Support:
                    proposal.SupportTokens += votingPower;
                    break;
                case VoteChoice.Against:
                    proposal.AgainstTokens += votingPower;
                    break;
                case VoteChoice.Abstain:
                    proposal.AbstainTokens += votingPower;
                    break;
            }

            await _context.SaveChangesAsync();

            // Check if proposal should be executed
            await CheckProposalExecution(proposal);

            _logger.LogInformation("Vote cast in DecVCPlat proposal {ProposalId}: {Choice} by user {UserId}", 
                request.ProposalId, request.Choice, userId.Value);

            return Ok(new { 
                message = "Vote cast successfully", 
                supportPercentage = proposal.SupportPercentage,
                againstPercentage = proposal.AgainstPercentage,
                hasMetQuorum = proposal.HasMetQuorum 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error casting vote in DecVCPlat proposal {ProposalId}", request.ProposalId);
            return BadRequest("Failed to cast vote");
        }
    }

    [HttpPost("proposals/{id:guid}/execute")]
    [Authorize(Policy = "DecVCPlat-Luminary")]
    public async Task<IActionResult> ExecuteProposal(Guid id)
    {
        try
        {
            var proposal = await _context.VotingProposals.FindAsync(id);
            if (proposal == null) return NotFound("Proposal not found");

            if (!proposal.CanExecute)
                return BadRequest("Proposal cannot be executed - either hasn't met quorum, hasn't expired, or is not active");

            proposal.Status = proposal.HasMetQuorum ? VotingStatus.Passed : VotingStatus.Failed;
            proposal.ExecutedAt = DateTime.UtcNow;
            proposal.ExecutionNotes = proposal.HasMetQuorum ? "Proposal passed and executed" : "Proposal failed to meet quorum";

            await _context.SaveChangesAsync();

            _logger.LogInformation("DecVCPlat proposal executed: {ProposalId} with status {Status}", 
                id, proposal.Status);

            return Ok(new { message = $"Proposal {proposal.Status.ToString().ToLower()}", status = proposal.Status });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing DecVCPlat proposal {ProposalId}", id);
            return BadRequest("Failed to execute proposal");
        }
    }

    private async Task CheckProposalExecution(VotingProposal proposal)
    {
        if (proposal.CanExecute)
        {
            proposal.Status = proposal.HasMetQuorum ? VotingStatus.Passed : VotingStatus.Failed;
            proposal.ExecutedAt = DateTime.UtcNow;
            proposal.ExecutionNotes = "Auto-executed after voting period ended";
            await _context.SaveChangesAsync();
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
}
