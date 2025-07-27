using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DecVCPlat.ProjectManagement.API.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FluentAssertions;

namespace DecVCPlat.ProjectManagement.Tests.Controllers
{
    public class ProjectsControllerTests
    {
        private readonly Mock<ILogger<ProjectsController>> _loggerMock;
        private readonly ProjectsController _controller;

        public ProjectsControllerTests()
        {
            _loggerMock = new Mock<ILogger<ProjectsController>>();
            _controller = new ProjectsController(_loggerMock.Object);
        }

        [Fact]
        public void GetProjects_ReturnsOkResult_WithListOfProjects()
        {
            // Act
            var result = _controller.GetProjects();

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var projects = okResult.Value.Should().BeAssignableTo<IEnumerable<ProjectDto>>().Subject;
            projects.Should().NotBeEmpty();
            projects.Should().HaveCount(2);
        }

        [Fact]
        public void GetProjects_WithFiltering_ReturnsOkResult_WithFilteredProjects()
        {
            // Arrange
            string status = "Active";
            string category = "DeFi";

            // Act
            var result = _controller.GetProjects(status, category);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var projects = okResult.Value.Should().BeAssignableTo<IEnumerable<ProjectDto>>().Subject;
            projects.Should().NotBeEmpty();
        }

        [Fact]
        public void GetProjects_WithPagination_ReturnsOkResult_WithPaginatedProjects()
        {
            // Arrange
            int pageNumber = 2;
            int pageSize = 5;

            // Act
            var result = _controller.GetProjects(pageNumber: pageNumber, pageSize: pageSize);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var projects = okResult.Value.Should().BeAssignableTo<IEnumerable<ProjectDto>>().Subject;
            projects.Should().NotBeEmpty();
        }

        [Fact]
        public void GetProject_WithValidId_ReturnsOkResult_WithProject()
        {
            // Arrange
            string projectId = "1";

            // Act
            var result = _controller.GetProject(projectId);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var project = okResult.Value.Should().BeAssignableTo<ProjectDto>().Subject;
            project.Id.Should().Be(projectId);
            project.Title.Should().NotBeNullOrEmpty();
            project.Description.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void CreateProject_WithValidRequest_ReturnsCreatedAtActionResult()
        {
            // Arrange
            var request = new CreateProjectRequest
            {
                Title = "Test Project",
                Description = "A test project for unit testing",
                Category = "Test",
                FundingGoal = 10000
            };

            // Act
            var result = _controller.CreateProject(request);

            // Assert
            var createdAtActionResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
            createdAtActionResult.ActionName.Should().Be(nameof(ProjectsController.GetProject));
            var project = createdAtActionResult.Value.Should().BeAssignableTo<ProjectDto>().Subject;
            project.Title.Should().Be(request.Title);
            project.Description.Should().Be(request.Description);
            project.Category.Should().Be(request.Category);
            project.FundingGoal.Should().Be(request.FundingGoal);
        }

        [Fact]
        public void UpdateProject_WithValidIdAndRequest_ReturnsOkResult()
        {
            // Arrange
            string projectId = "1";
            var request = new UpdateProjectRequest
            {
                Title = "Updated Project",
                Description = "Updated description",
                Status = "Completed"
            };

            // Act
            var result = _controller.UpdateProject(projectId, request);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var project = okResult.Value.Should().BeAssignableTo<ProjectDto>().Subject;
            project.Id.Should().Be(projectId);
            project.Title.Should().Be(request.Title);
            project.Description.Should().Be(request.Description);
            project.Status.Should().Be(request.Status);
        }

        [Fact]
        public void DeleteProject_WithValidId_ReturnsNoContentResult()
        {
            // Arrange
            string projectId = "1";

            // Act
            var result = _controller.DeleteProject(projectId);

            // Assert
            result.Should().BeOfType<NoContentResult>();
        }

        [Fact]
        public void GetProject_WithInvalidId_ReturnsNotFoundResult()
        {
            // Arrange
            string projectId = "999"; // Non-existent ID

            // Act
            var result = _controller.GetProject(projectId);

            // Assert
            result.Result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public void GetProjects_ThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ProjectsController>>();
            loggerMock
                .Setup(x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
                .Throws(new Exception("Test exception"));

            var controller = new ProjectsController(loggerMock.Object);

            // Act
            var result = controller.GetProjects();

            // Assert
            var statusCodeResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        }
    }
}
