// © 2024 DecVCPlat. All rights reserved.

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Shared.Security.Models;
using Shared.Security.Services;
using UserManagement.API.Models.DTOs;
using Shared.Common.Enums;
using FluentValidation;
using System.Security.Claims;

namespace UserManagement.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ILogger<AuthController> _logger;
    private readonly IValidator<RegisterRequest> _registerValidator;
    private readonly IValidator<LoginRequest> _loginValidator;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IJwtTokenService jwtTokenService,
        ILogger<AuthController> logger,
        IValidator<RegisterRequest> registerValidator,
        IValidator<LoginRequest> loginValidator)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtTokenService = jwtTokenService;
        _logger = logger;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
    }

    /// <summary>
    /// Register a new user for DecVCPlat platform
    /// </summary>
    /// <param name="request">Registration details including role (Founder/Investor/Luminary)</param>
    /// <returns>Authentication response with JWT token</returns>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthenticationResponse), 200)]
    [ProducesResponseType(typeof(AuthenticationResponse), 400)]
    public async Task<ActionResult<AuthenticationResponse>> Register([FromBody] RegisterRequest request)
    {
        try
        {
            var validationResult = await _registerValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                return BadRequest(new AuthenticationResponse
                {
                    IsSuccess = false,
                    Message = "Validation failed",
                    Errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList()
                });
            }

            // Check if user already exists
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
            {
                return BadRequest(new AuthenticationResponse
                {
                    IsSuccess = false,
                    Message = "User already exists with this email address",
                    Errors = new List<string> { "Email is already registered" }
                });
            }

            // Create new DecVCPlat user
            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                CompanyName = request.CompanyName,
                Bio = request.Bio,
                Role = request.Role,
                WalletAddress = request.WalletAddress,
                LinkedInProfile = request.LinkedInProfile,
                GitHubProfile = request.GitHubProfile,
                TwitterProfile = request.TwitterProfile,
                MinInvestmentAmount = request.MinInvestmentAmount,
                MaxInvestmentAmount = request.MaxInvestmentAmount,
                PreferredCategories = request.PreferredCategories,
                HasPreviousExperience = request.HasPreviousExperience,
                PreviousExperience = request.PreviousExperience,
                IsDataProcessingConsented = request.IsDataProcessingConsented,
                ConsentGivenAt = request.IsDataProcessingConsented ? DateTime.UtcNow : null,
                EmailConfirmed = false,
                Status = UserStatus.Active
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                return BadRequest(new AuthenticationResponse
                {
                    IsSuccess = false,
                    Message = "Failed to create user account",
                    Errors = result.Errors.Select(e => e.Description).ToList()
                });
            }

            // Add role claim for DecVCPlat
            await _userManager.AddClaimAsync(user, new Claim("role", request.Role.ToString()));
            await _userManager.AddClaimAsync(user, new Claim("platform", "DecVCPlat"));
            
            // Generate JWT token
            var token = _jwtTokenService.GenerateToken(user);
            var tokenExpiration = DateTime.UtcNow.AddDays(1); // 24 hours

            _logger.LogInformation("DecVCPlat user registered successfully: {Email} as {Role}", request.Email, request.Role);

            return Ok(new AuthenticationResponse
            {
                IsSuccess = true,
                Message = "Registration successful",
                Token = token,
                TokenExpiration = tokenExpiration,
                User = MapToUserResponse(user)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during user registration for email: {Email}", request.Email);
            return BadRequest(new AuthenticationResponse
            {
                IsSuccess = false,
                Message = "Registration failed due to internal error",
                Errors = new List<string> { "An unexpected error occurred" }
            });
        }
    }

    /// <summary>
    /// Authenticate user login for DecVCPlat platform
    /// </summary>
    /// <param name="request">Login credentials</param>
    /// <returns>Authentication response with JWT token</returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthenticationResponse), 200)]
    [ProducesResponseType(typeof(AuthenticationResponse), 401)]
    public async Task<ActionResult<AuthenticationResponse>> Login([FromBody] LoginRequest request)
    {
        try
        {
            var validationResult = await _loginValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                return BadRequest(new AuthenticationResponse
                {
                    IsSuccess = false,
                    Message = "Invalid login request",
                    Errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList()
                });
            }

            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null || user.Status == UserStatus.Inactive)
            {
                return Unauthorized(new AuthenticationResponse
                {
                    IsSuccess = false,
                    Message = "Invalid email or password",
                    Errors = new List<string> { "Authentication failed" }
                });
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
            if (!result.Succeeded)
            {
                var errorMessage = result.IsLockedOut ? "Account is locked" : "Invalid email or password";
                return Unauthorized(new AuthenticationResponse
                {
                    IsSuccess = false,
                    Message = errorMessage,
                    Errors = new List<string> { "Authentication failed" }
                });
            }

            // Generate JWT token for DecVCPlat
            var token = _jwtTokenService.GenerateToken(user);
            var tokenExpiration = DateTime.UtcNow.AddDays(1);

            // Update last login
            user.UpdatedAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            _logger.LogInformation("DecVCPlat user logged in successfully: {Email}", request.Email);

            return Ok(new AuthenticationResponse
            {
                IsSuccess = true,
                Message = "Login successful",
                Token = token,
                TokenExpiration = tokenExpiration,
                User = MapToUserResponse(user)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for email: {Email}", request.Email);
            return BadRequest(new AuthenticationResponse
            {
                IsSuccess = false,
                Message = "Login failed due to internal error",
                Errors = new List<string> { "An unexpected error occurred" }
            });
        }
    }

    /// <summary>
    /// Get current user profile information
    /// </summary>
    /// <returns>User profile data</returns>
    [HttpGet("profile")]
    [Authorize]
    [ProducesResponseType(typeof(UserResponse), 200)]
    [ProducesResponseType(401)]
    public async Task<ActionResult<UserResponse>> GetProfile()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
            {
                return Unauthorized();
            }

            var user = await _userManager.FindByIdAsync(userGuid.ToString());
            if (user == null)
            {
                return NotFound();
            }

            return Ok(MapToUserResponse(user));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user profile");
            return BadRequest("Failed to retrieve profile");
        }
    }

    /// <summary>
    /// Update wallet address for blockchain integration
    /// </summary>
    /// <param name="walletAddress">New wallet address</param>
    /// <returns>Success response</returns>
    [HttpPut("wallet")]
    [Authorize]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> UpdateWallet([FromBody] string walletAddress)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
            {
                return Unauthorized();
            }

            var user = await _userManager.FindByIdAsync(userGuid.ToString());
            if (user == null)
            {
                return NotFound();
            }

            // Basic wallet address validation
            if (string.IsNullOrWhiteSpace(walletAddress) || walletAddress.Length < 26)
            {
                return BadRequest("Invalid wallet address format");
            }

            user.WalletAddress = walletAddress;
            user.IsWalletVerified = false; // Reset verification on address change
            user.UpdatedAt = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest("Failed to update wallet address");
            }

            _logger.LogInformation("Wallet address updated for user: {UserId}", userId);
            return Ok(new { message = "Wallet address updated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating wallet address");
            return BadRequest("Failed to update wallet address");
        }
    }

    /// <summary>
    /// Logout user (invalidate token client-side)
    /// </summary>
    /// <returns>Success response</returns>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(200)]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return Ok(new { message = "Logout successful" });
    }

    private static UserResponse MapToUserResponse(ApplicationUser user)
    {
        return new UserResponse
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            UserName = user.UserName ?? string.Empty,
            FirstName = user.FirstName,
            LastName = user.LastName,
            CompanyName = user.CompanyName,
            Bio = user.Bio,
            Role = user.Role,
            Status = user.Status,
            WalletAddress = user.WalletAddress,
            IsWalletVerified = user.IsWalletVerified,
            IsKYCVerified = user.IsKYCVerified,
            TwoFactorEnabled = user.TwoFactorEnabled,
            CreatedAt = user.CreatedAt,
            LinkedInProfile = user.LinkedInProfile,
            GitHubProfile = user.GitHubProfile,
            TwitterProfile = user.TwitterProfile,
            MinInvestmentAmount = user.MinInvestmentAmount,
            MaxInvestmentAmount = user.MaxInvestmentAmount,
            PreferredCategories = user.PreferredCategories,
            HasPreviousExperience = user.HasPreviousExperience
        };
    }
}