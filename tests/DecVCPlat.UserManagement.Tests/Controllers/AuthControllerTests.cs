// © 2024 DecVCPlat. All rights reserved.

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using DecVCPlat.UserManagement.Controllers;
using DecVCPlat.UserManagement.DTOs;
using DecVCPlat.UserManagement.Services;
using DecVCPlat.Shared.Models;

namespace DecVCPlat.UserManagement.Tests.Controllers;

public class DecVCPlatAuthControllerTests
{
    private readonly Mock<IDecVCPlatUserService> _mockUserService;
    private readonly Mock<IDecVCPlatJwtTokenService> _mockTokenService;
    private readonly Mock<ILogger<DecVCPlatAuthController>> _mockLogger;
    private readonly DecVCPlatAuthController _controller;

    public DecVCPlatAuthControllerTests()
    {
        _mockUserService = new Mock<IDecVCPlatUserService>();
        _mockTokenService = new Mock<IDecVCPlatJwtTokenService>();
        _mockLogger = new Mock<ILogger<DecVCPlatAuthController>>();
        _controller = new DecVCPlatAuthController(_mockUserService.Object, _mockTokenService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task DecVCPlat_Register_ValidRequest_ReturnsOk()
    {
        // Arrange
        var registerRequest = new DecVCPlatRegisterRequest
        {
            Email = "test@decvcplat.com",
            Password = "DecVCPlat@123",
            FirstName = "John",
            LastName = "Doe",
            Role = "Founder",
            GdprConsent = true
        };

        var user = new DecVCPlatApplicationUser 
        { 
            Id = "user123", 
            Email = registerRequest.Email,
            FirstName = registerRequest.FirstName,
            LastName = registerRequest.LastName
        };

        _mockUserService.Setup(s => s.RegisterUserAsync(It.IsAny<DecVCPlatRegisterRequest>()))
            .ReturnsAsync(user);

        _mockTokenService.Setup(s => s.GenerateJwtToken(It.IsAny<DecVCPlatApplicationUser>()))
            .Returns("mock-jwt-token");

        // Act
        var result = await _controller.Register(registerRequest);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<DecVCPlatAuthenticationResponse>(okResult.Value);
        Assert.Equal("mock-jwt-token", response.Token);
        Assert.Equal(registerRequest.Email, response.Email);
    }

    [Fact]
    public async Task DecVCPlat_Register_InvalidRequest_ReturnsBadRequest()
    {
        // Arrange
        var registerRequest = new DecVCPlatRegisterRequest
        {
            Email = "invalid-email",
            Password = "weak",
            GdprConsent = false // Missing GDPR consent
        };

        // Act
        var result = await _controller.Register(registerRequest);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task DecVCPlat_Login_ValidCredentials_ReturnsOk()
    {
        // Arrange
        var loginRequest = new DecVCPlatLoginRequest
        {
            Email = "test@decvcplat.com",
            Password = "DecVCPlat@123"
        };

        var user = new DecVCPlatApplicationUser 
        { 
            Id = "user123", 
            Email = loginRequest.Email,
            FirstName = "John",
            LastName = "Doe"
        };

        _mockUserService.Setup(s => s.AuthenticateAsync(loginRequest.Email, loginRequest.Password))
            .ReturnsAsync(user);

        _mockTokenService.Setup(s => s.GenerateJwtToken(user))
            .Returns("mock-jwt-token");

        // Act
        var result = await _controller.Login(loginRequest);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<DecVCPlatAuthenticationResponse>(okResult.Value);
        Assert.Equal("mock-jwt-token", response.Token);
        Assert.Equal(loginRequest.Email, response.Email);
    }

    [Fact]
    public async Task DecVCPlat_Login_InvalidCredentials_ReturnsUnauthorized()
    {
        // Arrange
        var loginRequest = new DecVCPlatLoginRequest
        {
            Email = "test@decvcplat.com",
            Password = "wrong-password"
        };

        _mockUserService.Setup(s => s.AuthenticateAsync(loginRequest.Email, loginRequest.Password))
            .ReturnsAsync((DecVCPlatApplicationUser?)null);

        // Act
        var result = await _controller.Login(loginRequest);

        // Assert
        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    [Theory]
    [InlineData("Founder")]
    [InlineData("Investor")]
    [InlineData("Luminary")]
    public async Task DecVCPlat_Register_ValidRoles_Success(string role)
    {
        // Arrange
        var registerRequest = new DecVCPlatRegisterRequest
        {
            Email = $"test-{role.ToLower()}@decvcplat.com",
            Password = "DecVCPlat@123",
            FirstName = "Test",
            LastName = "User",
            Role = role,
            GdprConsent = true
        };

        var user = new DecVCPlatApplicationUser 
        { 
            Id = $"user-{role}", 
            Email = registerRequest.Email,
            FirstName = registerRequest.FirstName,
            LastName = registerRequest.LastName
        };

        _mockUserService.Setup(s => s.RegisterUserAsync(It.IsAny<DecVCPlatRegisterRequest>()))
            .ReturnsAsync(user);

        _mockTokenService.Setup(s => s.GenerateJwtToken(It.IsAny<DecVCPlatApplicationUser>()))
            .Returns($"mock-jwt-token-{role}");

        // Act
        var result = await _controller.Register(registerRequest);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<DecVCPlatAuthenticationResponse>(okResult.Value);
        Assert.Equal($"mock-jwt-token-{role}", response.Token);
        Assert.Equal(registerRequest.Email, response.Email);
    }

    [Fact]
    public async Task DecVCPlat_Register_WithoutGdprConsent_ReturnsBadRequest()
    {
        // Arrange
        var registerRequest = new DecVCPlatRegisterRequest
        {
            Email = "test@decvcplat.com",
            Password = "DecVCPlat@123",
            FirstName = "John",
            LastName = "Doe",
            Role = "Founder",
            GdprConsent = false // No GDPR consent
        };

        // Act
        var result = await _controller.Register(registerRequest);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var errorMessage = badRequestResult.Value?.ToString();
        Assert.Contains("GDPR consent is required", errorMessage);
    }

    [Fact]
    public async Task DecVCPlat_Register_DuplicateEmail_ReturnsBadRequest()
    {
        // Arrange
        var registerRequest = new DecVCPlatRegisterRequest
        {
            Email = "existing@decvcplat.com",
            Password = "DecVCPlat@123",
            FirstName = "John",
            LastName = "Doe",
            Role = "Founder",
            GdprConsent = true
        };

        _mockUserService.Setup(s => s.RegisterUserAsync(It.IsAny<DecVCPlatRegisterRequest>()))
            .ThrowsAsync(new InvalidOperationException("User with this email already exists"));

        // Act
        var result = await _controller.Register(registerRequest);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var errorMessage = badRequestResult.Value?.ToString();
        Assert.Contains("User with this email already exists", errorMessage);
    }

    [Fact]
    public async Task DecVCPlat_GetProfile_AuthenticatedUser_ReturnsUserProfile()
    {
        // Arrange
        var userId = "user123";
        var user = new DecVCPlatApplicationUser
        {
            Id = userId,
            Email = "test@decvcplat.com",
            FirstName = "John",
            LastName = "Doe"
        };

        _mockUserService.Setup(s => s.GetUserByIdAsync(userId))
            .ReturnsAsync(user);

        // Mock User.Identity.Name to return userId
        var mockUser = new System.Security.Claims.ClaimsPrincipal(
            new System.Security.Claims.ClaimsIdentity(new[]
            {
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, userId)
            }, "mock"));

        _controller.ControllerContext = new ControllerContext()
        {
            HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext() { User = mockUser }
        };

        // Act
        var result = await _controller.GetProfile();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var profile = Assert.IsType<DecVCPlatUserProfileResponse>(okResult.Value);
        Assert.Equal(user.Email, profile.Email);
        Assert.Equal(user.FirstName, profile.FirstName);
        Assert.Equal(user.LastName, profile.LastName);
    }

    [Fact]
    public async Task DecVCPlat_UpdateWalletAddress_ValidAddress_ReturnsOk()
    {
        // Arrange
        var userId = "user123";
        var walletRequest = new DecVCPlatUpdateWalletRequest
        {
            WalletAddress = "0x1234567890123456789012345678901234567890"
        };

        _mockUserService.Setup(s => s.UpdateWalletAddressAsync(userId, walletRequest.WalletAddress))
            .ReturnsAsync(true);

        // Mock authenticated user
        var mockUser = new System.Security.Claims.ClaimsPrincipal(
            new System.Security.Claims.ClaimsIdentity(new[]
            {
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, userId)
            }, "mock"));

        _controller.ControllerContext = new ControllerContext()
        {
            HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext() { User = mockUser }
        };

        // Act
        var result = await _controller.UpdateWalletAddress(walletRequest);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = okResult.Value?.ToString();
        Assert.Contains("Wallet address updated successfully", response);
    }

    [Fact]
    public async Task DecVCPlat_UpdateWalletAddress_InvalidAddress_ReturnsBadRequest()
    {
        // Arrange
        var userId = "user123";
        var walletRequest = new DecVCPlatUpdateWalletRequest
        {
            WalletAddress = "invalid-wallet-address"
        };

        // Mock authenticated user
        var mockUser = new System.Security.Claims.ClaimsPrincipal(
            new System.Security.Claims.ClaimsIdentity(new[]
            {
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, userId)
            }, "mock"));

        _controller.ControllerContext = new ControllerContext()
        {
            HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext() { User = mockUser }
        };

        // Act
        var result = await _controller.UpdateWalletAddress(walletRequest);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }
}
