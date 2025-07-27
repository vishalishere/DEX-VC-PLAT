using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DecVCPlat.Voting.API.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FluentAssertions;

namespace DecVCPlat.Voting.Tests.Controllers
{
    public class VotesControllerTests
    {
        private readonly Mock<ILogger<VotesController>> _loggerMock;
        private readonly VotesController _controller;

        public VotesControllerTests()
        {
            _loggerMock = new Mock<ILogger<VotesController>>();
            _controller = new VotesController(_loggerMock.Object);
        }

        [Fact]
        public void GetVotesByProject_WithValidProjectId_ReturnsOkResult_WithListOfVotes()
        {
            // Arrange
            string projectId = "project-1";

            // Act
            var result = _controller.GetVotesByProject(projectId);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var votes = okResult.Value.Should().BeAssignableTo<IEnumerable<VoteDto>>().Subject;
            votes.Should().NotBeEmpty();
            votes.Should().HaveCount(2);
            votes.All(v => v.ProjectId == projectId).Should().BeTrue();
        }

        [Fact]
        public void GetVotesByProject_WithPagination_ReturnsOkResult_WithPaginatedVotes()
        {
            // Arrange
            string projectId = "project-1";
            int pageNumber = 2;
            int pageSize = 5;

            // Act
            var result = _controller.GetVotesByProject(projectId, pageNumber, pageSize);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var votes = okResult.Value.Should().BeAssignableTo<IEnumerable<VoteDto>>().Subject;
            votes.Should().NotBeEmpty();
        }

        [Fact]
        public void GetVote_WithValidId_ReturnsOkResult_WithVote()
        {
            // Arrange
            string voteId = "1";

            // Act
            var result = _controller.GetVote(voteId);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var vote = okResult.Value.Should().BeAssignableTo<VoteDto>().Subject;
            vote.Id.Should().Be(voteId);
        }

        [Fact]
        public void GetVote_WithInvalidId_ReturnsNotFoundResult()
        {
            // Arrange
            string voteId = "999"; // Non-existent ID

            // Act
            var result = _controller.GetVote(voteId);

            // Assert
            result.Result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public void CreateVote_WithValidRequest_ReturnsCreatedAtActionResult()
        {
            // Arrange
            var request = new CreateVoteRequest
            {
                ProjectId = "project-1",
                VoteType = VoteType.Positive,
                Weight = 1,
                Comment = "Great project!"
            };

            // Act
            var result = _controller.CreateVote(request);

            // Assert
            var createdAtActionResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
            createdAtActionResult.ActionName.Should().Be(nameof(VotesController.GetVote));
            var vote = createdAtActionResult.Value.Should().BeAssignableTo<VoteDto>().Subject;
            vote.ProjectId.Should().Be(request.ProjectId);
            vote.VoteType.Should().Be(request.VoteType);
            vote.Weight.Should().Be(request.Weight);
            vote.Comment.Should().Be(request.Comment);
        }

        [Fact]
        public void UpdateVote_WithValidIdAndRequest_ReturnsOkResult()
        {
            // Arrange
            string voteId = "1";
            var request = new UpdateVoteRequest
            {
                VoteType = VoteType.Neutral,
                Comment = "Updated comment"
            };

            // Act
            var result = _controller.UpdateVote(voteId, request);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var vote = okResult.Value.Should().BeAssignableTo<VoteDto>().Subject;
            vote.Id.Should().Be(voteId);
            vote.VoteType.Should().Be(request.VoteType);
            vote.Comment.Should().Be(request.Comment);
        }

        [Fact]
        public void DeleteVote_WithValidId_ReturnsNoContentResult()
        {
            // Arrange
            string voteId = "1";

            // Act
            var result = _controller.DeleteVote(voteId);

            // Assert
            result.Should().BeOfType<NoContentResult>();
        }

        [Fact]
        public void GetVotingSummary_WithValidProjectId_ReturnsOkResult_WithSummary()
        {
            // Arrange
            string projectId = "project-1";

            // Act
            var result = _controller.GetVotingSummary(projectId);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var summary = okResult.Value.Should().BeAssignableTo<VotingSummaryDto>().Subject;
            summary.ProjectId.Should().Be(projectId);
            summary.TotalVotes.Should().BeGreaterThan(0);
        }

        [Fact]
        public void GetVotesByProject_ThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<VotesController>>();
            loggerMock
                .Setup(x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
                .Throws(new Exception("Test exception"));

            var controller = new VotesController(loggerMock.Object);
            string projectId = "error";

            // Act
            var result = controller.GetVotesByProject(projectId);

            // Assert
            var statusCodeResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        }
    }
}
