using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace DecVCPlat.Voting.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class VotingController : ControllerBase
    {
        /// <summary>
        /// Retrieves all active voting sessions
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /api/voting
        ///
        /// </remarks>
        /// <returns>A list of all active voting sessions</returns>
        /// <response code="200">Returns the list of voting sessions</response>
        /// <response code="401">If the user is not authenticated</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<VotingSessionDto>), 200)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<IEnumerable<VotingSessionDto>>> GetVotingSessions()
        {
            // In a real implementation, this would retrieve voting sessions from a database
            var sessions = new List<VotingSessionDto>
            {
                new VotingSessionDto 
                { 
                    Id = 1, 
                    ProjectId = 1,
                    Title = "Funding Decision for DeFi Platform", 
                    Description = "Vote on whether to fund the Decentralized Finance Platform project", 
                    StartDate = DateTime.Now.AddDays(-5),
                    EndDate = DateTime.Now.AddDays(5),
                    Status = "Active",
                    TotalVotes = 15,
                    QuorumRequired = 20
                },
                new VotingSessionDto 
                { 
                    Id = 2, 
                    ProjectId = 2,
                    Title = "Funding Decision for Blockchain Supply Chain", 
                    Description = "Vote on whether to fund the Blockchain Supply Chain project", 
                    StartDate = DateTime.Now.AddDays(-3),
                    EndDate = DateTime.Now.AddDays(7),
                    Status = "Active",
                    TotalVotes = 8,
                    QuorumRequired = 20
                }
            };

            return Ok(sessions);
        }

        /// <summary>
        /// Retrieves a specific voting session by id
        /// </summary>
        /// <param name="id">The voting session id</param>
        /// <returns>The voting session details</returns>
        /// <response code="200">Returns the voting session</response>
        /// <response code="404">If the voting session is not found</response>
        /// <response code="401">If the user is not authenticated</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(VotingSessionDto), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<VotingSessionDto>> GetVotingSession(int id)
        {
            // In a real implementation, this would retrieve the voting session from a database
            if (id != 1 && id != 2)
            {
                return NotFound();
            }

            var session = id == 1
                ? new VotingSessionDto 
                { 
                    Id = 1, 
                    ProjectId = 1,
                    Title = "Funding Decision for DeFi Platform", 
                    Description = "Vote on whether to fund the Decentralized Finance Platform project", 
                    StartDate = DateTime.Now.AddDays(-5),
                    EndDate = DateTime.Now.AddDays(5),
                    Status = "Active",
                    TotalVotes = 15,
                    QuorumRequired = 20
                }
                : new VotingSessionDto 
                { 
                    Id = 2, 
                    ProjectId = 2,
                    Title = "Funding Decision for Blockchain Supply Chain", 
                    Description = "Vote on whether to fund the Blockchain Supply Chain project", 
                    StartDate = DateTime.Now.AddDays(-3),
                    EndDate = DateTime.Now.AddDays(7),
                    Status = "Active",
                    TotalVotes = 8,
                    QuorumRequired = 20
                };

            return Ok(session);
        }

        /// <summary>
        /// Creates a new voting session
        /// </summary>
        /// <param name="createVotingSessionDto">The voting session creation data</param>
        /// <returns>The newly created voting session</returns>
        /// <response code="201">Returns the newly created voting session</response>
        /// <response code="400">If the request data is invalid</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="403">If the user is not authorized to create a voting session</response>
        [HttpPost]
        [ProducesResponseType(typeof(VotingSessionDto), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<VotingSessionDto>> CreateVotingSession(CreateVotingSessionDto createVotingSessionDto)
        {
            // In a real implementation, this would create a new voting session in the database
            var newSession = new VotingSessionDto
            {
                Id = 3,
                ProjectId = createVotingSessionDto.ProjectId,
                Title = createVotingSessionDto.Title,
                Description = createVotingSessionDto.Description,
                StartDate = createVotingSessionDto.StartDate,
                EndDate = createVotingSessionDto.EndDate,
                Status = "Pending",
                TotalVotes = 0,
                QuorumRequired = createVotingSessionDto.QuorumRequired
            };

            return CreatedAtAction(nameof(GetVotingSession), new { id = newSession.Id }, newSession);
        }

        /// <summary>
        /// Casts a vote in a voting session
        /// </summary>
        /// <param name="id">The voting session id</param>
        /// <param name="voteDto">The vote data</param>
        /// <returns>The updated vote count</returns>
        /// <response code="200">Returns the updated vote count</response>
        /// <response code="400">If the request data is invalid</response>
        /// <response code="404">If the voting session is not found</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="403">If the user is not authorized to vote or has already voted</response>
        /// <response code="409">If the voting session is not active</response>
        [HttpPost("{id}/vote")]
        [ProducesResponseType(typeof(VoteResultDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(409)]
        public async Task<ActionResult<VoteResultDto>> CastVote(int id, VoteDto voteDto)
        {
            // In a real implementation, this would record the vote in the database
            if (id != 1 && id != 2)
            {
                return NotFound();
            }

            // Check if voting session is active
            // Check if user has already voted
            // Record the vote

            var result = new VoteResultDto
            {
                VotingSessionId = id,
                UserId = 1, // Assuming the current user's ID
                VoteChoice = voteDto.VoteChoice,
                Timestamp = DateTime.Now,
                TotalVotes = id == 1 ? 16 : 9,
                QuorumRequired = 20,
                QuorumReached = false
            };

            return Ok(result);
        }

        /// <summary>
        /// Gets the results of a completed voting session
        /// </summary>
        /// <param name="id">The voting session id</param>
        /// <returns>The voting session results</returns>
        /// <response code="200">Returns the voting session results</response>
        /// <response code="404">If the voting session is not found</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="409">If the voting session is not completed</response>
        [HttpGet("{id}/results")]
        [ProducesResponseType(typeof(VotingResultsDto), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(401)]
        [ProducesResponseType(409)]
        public async Task<ActionResult<VotingResultsDto>> GetVotingResults(int id)
        {
            // In a real implementation, this would retrieve the voting results from a database
            if (id != 1 && id != 2)
            {
                return NotFound();
            }

            // Check if voting session is completed
            // If not, return 409 Conflict

            var results = new VotingResultsDto
            {
                VotingSessionId = id,
                ProjectId = id,
                Title = id == 1 ? "Funding Decision for DeFi Platform" : "Funding Decision for Blockchain Supply Chain",
                TotalVotes = id == 1 ? 16 : 9,
                YesVotes = id == 1 ? 12 : 6,
                NoVotes = id == 1 ? 4 : 3,
                QuorumRequired = 20,
                QuorumReached = false,
                Approved = false,
                CompletedAt = DateTime.Now
            };

            return Ok(results);
        }
    }

    public class VotingSessionDto
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; }
        public int TotalVotes { get; set; }
        public int QuorumRequired { get; set; }
    }

    public class CreateVotingSessionDto
    {
        [Required]
        public int ProjectId { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        [Required]
        [StringLength(1000)]
        public string Description { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int QuorumRequired { get; set; }
    }

    public class VoteDto
    {
        [Required]
        public bool VoteChoice { get; set; }
    }

    public class VoteResultDto
    {
        public int VotingSessionId { get; set; }
        public int UserId { get; set; }
        public bool VoteChoice { get; set; }
        public DateTime Timestamp { get; set; }
        public int TotalVotes { get; set; }
        public int QuorumRequired { get; set; }
        public bool QuorumReached { get; set; }
    }

    public class VotingResultsDto
    {
        public int VotingSessionId { get; set; }
        public int ProjectId { get; set; }
        public string Title { get; set; }
        public int TotalVotes { get; set; }
        public int YesVotes { get; set; }
        public int NoVotes { get; set; }
        public int QuorumRequired { get; set; }
        public bool QuorumReached { get; set; }
        public bool Approved { get; set; }
        public DateTime CompletedAt { get; set; }
    }
}
