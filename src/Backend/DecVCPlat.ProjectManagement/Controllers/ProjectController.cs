using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace DecVCPlat.ProjectManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProjectController : ControllerBase
    {
        /// <summary>
        /// Retrieves all projects in the system
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /api/project
        ///
        /// </remarks>
        /// <returns>A list of all projects</returns>
        /// <response code="200">Returns the list of projects</response>
        /// <response code="401">If the user is not authenticated</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ProjectDto>), 200)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<IEnumerable<ProjectDto>>> GetProjects()
        {
            // In a real implementation, this would retrieve projects from a database
            var projects = new List<ProjectDto>
            {
                new ProjectDto 
                { 
                    Id = 1, 
                    Title = "Decentralized Finance Platform", 
                    Description = "A platform for decentralized finance operations", 
                    FundingGoal = 1000000, 
                    CurrentFunding = 750000,
                    OwnerId = 2,
                    Status = "Active",
                    CreatedAt = DateTime.Now.AddMonths(-2),
                    UpdatedAt = DateTime.Now.AddDays(-5)
                },
                new ProjectDto 
                { 
                    Id = 2, 
                    Title = "Blockchain Supply Chain", 
                    Description = "Supply chain management using blockchain technology", 
                    FundingGoal = 500000, 
                    CurrentFunding = 200000,
                    OwnerId = 3,
                    Status = "Active",
                    CreatedAt = DateTime.Now.AddMonths(-1),
                    UpdatedAt = DateTime.Now.AddDays(-2)
                }
            };

            return Ok(projects);
        }

        /// <summary>
        /// Retrieves a specific project by id
        /// </summary>
        /// <param name="id">The project id</param>
        /// <returns>The project details</returns>
        /// <response code="200">Returns the project</response>
        /// <response code="404">If the project is not found</response>
        /// <response code="401">If the user is not authenticated</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ProjectDto), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<ProjectDto>> GetProject(int id)
        {
            // In a real implementation, this would retrieve the project from a database
            if (id != 1 && id != 2)
            {
                return NotFound();
            }

            var project = id == 1
                ? new ProjectDto 
                { 
                    Id = 1, 
                    Title = "Decentralized Finance Platform", 
                    Description = "A platform for decentralized finance operations", 
                    FundingGoal = 1000000, 
                    CurrentFunding = 750000,
                    OwnerId = 2,
                    Status = "Active",
                    CreatedAt = DateTime.Now.AddMonths(-2),
                    UpdatedAt = DateTime.Now.AddDays(-5)
                }
                : new ProjectDto 
                { 
                    Id = 2, 
                    Title = "Blockchain Supply Chain", 
                    Description = "Supply chain management using blockchain technology", 
                    FundingGoal = 500000, 
                    CurrentFunding = 200000,
                    OwnerId = 3,
                    Status = "Active",
                    CreatedAt = DateTime.Now.AddMonths(-1),
                    UpdatedAt = DateTime.Now.AddDays(-2)
                };

            return Ok(project);
        }

        /// <summary>
        /// Creates a new project
        /// </summary>
        /// <param name="createProjectDto">The project creation data</param>
        /// <returns>The newly created project</returns>
        /// <response code="201">Returns the newly created project</response>
        /// <response code="400">If the request data is invalid</response>
        /// <response code="401">If the user is not authenticated</response>
        [HttpPost]
        [ProducesResponseType(typeof(ProjectDto), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<ProjectDto>> CreateProject(CreateProjectDto createProjectDto)
        {
            // In a real implementation, this would create a new project in the database
            var newProject = new ProjectDto
            {
                Id = 3,
                Title = createProjectDto.Title,
                Description = createProjectDto.Description,
                FundingGoal = createProjectDto.FundingGoal,
                CurrentFunding = 0,
                OwnerId = 2, // Assuming the current user's ID
                Status = "Pending",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            return CreatedAtAction(nameof(GetProject), new { id = newProject.Id }, newProject);
        }

        /// <summary>
        /// Updates an existing project
        /// </summary>
        /// <param name="id">The project id</param>
        /// <param name="updateProjectDto">The updated project data</param>
        /// <returns>No content</returns>
        /// <response code="204">If the project was updated successfully</response>
        /// <response code="400">If the request data is invalid</response>
        /// <response code="404">If the project is not found</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="403">If the user is not authorized to update the project</response>
        [HttpPut("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> UpdateProject(int id, UpdateProjectDto updateProjectDto)
        {
            // In a real implementation, this would update the project in the database
            if (id != 1 && id != 2)
            {
                return NotFound();
            }

            // Authorization check would go here
            // Update logic would go here

            return NoContent();
        }

        /// <summary>
        /// Deletes a specific project
        /// </summary>
        /// <param name="id">The project id</param>
        /// <returns>No content</returns>
        /// <response code="204">If the project was deleted successfully</response>
        /// <response code="404">If the project is not found</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="403">If the user is not authorized to delete the project</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> DeleteProject(int id)
        {
            // In a real implementation, this would delete the project from the database
            if (id != 1 && id != 2)
            {
                return NotFound();
            }

            // Authorization check would go here
            // Delete logic would go here

            return NoContent();
        }

        /// <summary>
        /// Submits a project for review
        /// </summary>
        /// <param name="id">The project id</param>
        /// <returns>The updated project status</returns>
        /// <response code="200">Returns the updated project status</response>
        /// <response code="404">If the project is not found</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="403">If the user is not authorized to submit the project</response>
        [HttpPost("{id}/submit")]
        [ProducesResponseType(typeof(ProjectStatusDto), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<ProjectStatusDto>> SubmitProject(int id)
        {
            // In a real implementation, this would update the project status in the database
            if (id != 1 && id != 2)
            {
                return NotFound();
            }

            // Authorization check would go here
            // Status update logic would go here

            var status = new ProjectStatusDto
            {
                ProjectId = id,
                Status = "Under Review",
                UpdatedAt = DateTime.Now
            };

            return Ok(status);
        }
    }

    public class ProjectDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal FundingGoal { get; set; }
        public decimal CurrentFunding { get; set; }
        public int OwnerId { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CreateProjectDto
    {
        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        [Required]
        [StringLength(5000)]
        public string Description { get; set; }

        [Required]
        [Range(1, double.MaxValue)]
        public decimal FundingGoal { get; set; }
    }

    public class UpdateProjectDto
    {
        [StringLength(100)]
        public string Title { get; set; }

        [StringLength(5000)]
        public string Description { get; set; }

        [Range(1, double.MaxValue)]
        public decimal? FundingGoal { get; set; }
    }

    public class ProjectStatusDto
    {
        public int ProjectId { get; set; }
        public string Status { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
