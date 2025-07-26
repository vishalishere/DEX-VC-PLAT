using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace DecVCPlat.UserManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        /// <summary>
        /// Retrieves all users in the system
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /api/user
        ///
        /// </remarks>
        /// <returns>A list of all users</returns>
        /// <response code="200">Returns the list of users</response>
        /// <response code="401">If the user is not authenticated</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<UserDto>), 200)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
        {
            // In a real implementation, this would retrieve users from a database
            var users = new List<UserDto>
            {
                new UserDto { Id = 1, Username = "john.doe", Email = "john.doe@example.com", FullName = "John Doe", Role = "Investor" },
                new UserDto { Id = 2, Username = "jane.smith", Email = "jane.smith@example.com", FullName = "Jane Smith", Role = "ProjectOwner" }
            };

            return Ok(users);
        }

        /// <summary>
        /// Retrieves a specific user by id
        /// </summary>
        /// <param name="id">The user id</param>
        /// <returns>The user details</returns>
        /// <response code="200">Returns the user</response>
        /// <response code="404">If the user is not found</response>
        /// <response code="401">If the user is not authenticated</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(UserDto), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<UserDto>> GetUser(int id)
        {
            // In a real implementation, this would retrieve the user from a database
            if (id != 1 && id != 2)
            {
                return NotFound();
            }

            var user = id == 1
                ? new UserDto { Id = 1, Username = "john.doe", Email = "john.doe@example.com", FullName = "John Doe", Role = "Investor" }
                : new UserDto { Id = 2, Username = "jane.smith", Email = "jane.smith@example.com", FullName = "Jane Smith", Role = "ProjectOwner" };

            return Ok(user);
        }

        /// <summary>
        /// Creates a new user
        /// </summary>
        /// <param name="createUserDto">The user creation data</param>
        /// <returns>The newly created user</returns>
        /// <response code="201">Returns the newly created user</response>
        /// <response code="400">If the request data is invalid</response>
        /// <response code="401">If the user is not authenticated</response>
        [HttpPost]
        [ProducesResponseType(typeof(UserDto), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<UserDto>> CreateUser(CreateUserDto createUserDto)
        {
            // In a real implementation, this would create a new user in the database
            var newUser = new UserDto
            {
                Id = 3,
                Username = createUserDto.Username,
                Email = createUserDto.Email,
                FullName = createUserDto.FullName,
                Role = createUserDto.Role
            };

            return CreatedAtAction(nameof(GetUser), new { id = newUser.Id }, newUser);
        }

        /// <summary>
        /// Updates an existing user
        /// </summary>
        /// <param name="id">The user id</param>
        /// <param name="updateUserDto">The updated user data</param>
        /// <returns>No content</returns>
        /// <response code="204">If the user was updated successfully</response>
        /// <response code="400">If the request data is invalid</response>
        /// <response code="404">If the user is not found</response>
        /// <response code="401">If the user is not authenticated</response>
        [HttpPut("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> UpdateUser(int id, UpdateUserDto updateUserDto)
        {
            // In a real implementation, this would update the user in the database
            if (id != 1 && id != 2)
            {
                return NotFound();
            }

            // Update logic would go here

            return NoContent();
        }

        /// <summary>
        /// Deletes a specific user
        /// </summary>
        /// <param name="id">The user id</param>
        /// <returns>No content</returns>
        /// <response code="204">If the user was deleted successfully</response>
        /// <response code="404">If the user is not found</response>
        /// <response code="401">If the user is not authenticated</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> DeleteUser(int id)
        {
            // In a real implementation, this would delete the user from the database
            if (id != 1 && id != 2)
            {
                return NotFound();
            }

            // Delete logic would go here

            return NoContent();
        }

        /// <summary>
        /// Authenticates a user and returns a JWT token
        /// </summary>
        /// <param name="loginDto">The login credentials</param>
        /// <returns>JWT token</returns>
        /// <response code="200">Returns the JWT token</response>
        /// <response code="401">If authentication fails</response>
        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(AuthResponseDto), 200)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<AuthResponseDto>> Login(LoginDto loginDto)
        {
            // In a real implementation, this would validate credentials and generate a JWT token
            if (loginDto.Username != "john.doe" || loginDto.Password != "password")
            {
                return Unauthorized();
            }

            var response = new AuthResponseDto
            {
                Token = "sample-jwt-token",
                ExpiresIn = 3600,
                UserId = 1
            };

            return Ok(response);
        }
    }

    public class UserDto
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string Role { get; set; }
    }

    public class CreateUserDto
    {
        [Required]
        [StringLength(50)]
        public string Username { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(100)]
        public string FullName { get; set; }

        [Required]
        [StringLength(50)]
        public string Role { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 8)]
        public string Password { get; set; }
    }

    public class UpdateUserDto
    {
        [EmailAddress]
        public string Email { get; set; }

        [StringLength(100)]
        public string FullName { get; set; }

        [StringLength(50)]
        public string Role { get; set; }
    }

    public class LoginDto
    {
        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }
    }

    public class AuthResponseDto
    {
        public string Token { get; set; }
        public int ExpiresIn { get; set; }
        public int UserId { get; set; }
    }
}
