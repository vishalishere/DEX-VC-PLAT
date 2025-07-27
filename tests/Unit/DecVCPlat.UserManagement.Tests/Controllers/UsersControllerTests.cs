using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DecVCPlat.UserManagement.API.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FluentAssertions;

namespace DecVCPlat.UserManagement.Tests.Controllers
{
    public class UsersControllerTests
    {
        private readonly Mock<ILogger<UsersController>> _loggerMock;
        private readonly UsersController _controller;

        public UsersControllerTests()
        {
            _loggerMock = new Mock<ILogger<UsersController>>();
            _controller = new UsersController(_loggerMock.Object);
        }

        [Fact]
        public void GetUsers_ReturnsOkResult_WithListOfUsers()
        {
            // Act
            var result = _controller.GetUsers();

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var users = okResult.Value.Should().BeAssignableTo<IEnumerable<UserDto>>().Subject;
            users.Should().NotBeEmpty();
            users.Should().HaveCount(2);
        }

        [Fact]
        public void GetUsers_WithPagination_ReturnsOkResult_WithListOfUsers()
        {
            // Arrange
            int pageNumber = 2;
            int pageSize = 5;

            // Act
            var result = _controller.GetUsers(pageNumber, pageSize);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var users = okResult.Value.Should().BeAssignableTo<IEnumerable<UserDto>>().Subject;
            users.Should().NotBeEmpty();
        }

        [Fact]
        public void GetUser_WithValidId_ReturnsOkResult_WithUser()
        {
            // Arrange
            string userId = "1";

            // Act
            var result = _controller.GetUser(userId);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var user = okResult.Value.Should().BeAssignableTo<UserDto>().Subject;
            user.Id.Should().Be(userId);
            user.Username.Should().Be($"user{userId}");
            user.Email.Should().Be($"user{userId}@example.com");
            user.IsActive.Should().BeTrue();
        }

        [Fact]
        public void CreateUser_WithValidRequest_ReturnsCreatedAtActionResult()
        {
            // Arrange
            var request = new CreateUserRequest
            {
                Username = "newuser",
                Email = "newuser@example.com",
                Password = "Password123!"
            };

            // Act
            var result = _controller.CreateUser(request);

            // Assert
            var createdAtActionResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
            createdAtActionResult.ActionName.Should().Be(nameof(UsersController.GetUser));
            var user = createdAtActionResult.Value.Should().BeAssignableTo<UserDto>().Subject;
            user.Username.Should().Be(request.Username);
            user.Email.Should().Be(request.Email);
        }

        [Fact]
        public void UpdateUser_WithValidIdAndRequest_ReturnsOkResult()
        {
            // Arrange
            string userId = "1";
            var request = new UpdateUserRequest
            {
                Email = "updated@example.com",
                IsActive = false
            };

            // Act
            var result = _controller.UpdateUser(userId, request);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var user = okResult.Value.Should().BeAssignableTo<UserDto>().Subject;
            user.Id.Should().Be(userId);
            user.Email.Should().Be(request.Email);
            user.IsActive.Should().Be(request.IsActive);
        }

        [Fact]
        public void DeleteUser_WithValidId_ReturnsNoContentResult()
        {
            // Arrange
            string userId = "1";

            // Act
            var result = _controller.DeleteUser(userId);

            // Assert
            result.Should().BeOfType<NoContentResult>();
        }

        [Fact]
        public void GetUser_ThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<UsersController>>();
            loggerMock
                .Setup(x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
                .Throws(new Exception("Test exception"));

            var controller = new UsersController(loggerMock.Object);
            string userId = "error";

            // Act
            var result = controller.GetUser(userId);

            // Assert
            var statusCodeResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        }
    }
}
