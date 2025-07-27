using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DecVCPlat.Funding.API.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FluentAssertions;

namespace DecVCPlat.Funding.Tests.Controllers
{
    public class FundingControllerTests
    {
        private readonly Mock<ILogger<FundingController>> _loggerMock;
        private readonly FundingController _controller;

        public FundingControllerTests()
        {
            _loggerMock = new Mock<ILogger<FundingController>>();
            _controller = new FundingController(_loggerMock.Object);
        }

        [Fact]
        public void GetFundingByProject_WithValidProjectId_ReturnsOkResult_WithListOfTransactions()
        {
            // Arrange
            string projectId = "project-1";

            // Act
            var result = _controller.GetFundingByProject(projectId);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var transactions = okResult.Value.Should().BeAssignableTo<IEnumerable<FundingTransactionDto>>().Subject;
            transactions.Should().NotBeEmpty();
            transactions.Should().HaveCount(2);
            transactions.All(t => t.ProjectId == projectId).Should().BeTrue();
        }

        [Fact]
        public void GetFundingByProject_WithPagination_ReturnsOkResult_WithPaginatedTransactions()
        {
            // Arrange
            string projectId = "project-1";
            int pageNumber = 2;
            int pageSize = 5;

            // Act
            var result = _controller.GetFundingByProject(projectId, pageNumber, pageSize);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var transactions = okResult.Value.Should().BeAssignableTo<IEnumerable<FundingTransactionDto>>().Subject;
            transactions.Should().NotBeEmpty();
        }

        [Fact]
        public void GetFundingTransaction_WithValidId_ReturnsOkResult_WithTransaction()
        {
            // Arrange
            string transactionId = "1";

            // Act
            var result = _controller.GetFundingTransaction(transactionId);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var transaction = okResult.Value.Should().BeAssignableTo<FundingTransactionDto>().Subject;
            transaction.Id.Should().Be(transactionId);
        }

        [Fact]
        public void GetFundingTransaction_WithInvalidId_ReturnsNotFoundResult()
        {
            // Arrange
            string transactionId = "999"; // Non-existent ID

            // Act
            var result = _controller.GetFundingTransaction(transactionId);

            // Assert
            result.Result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public void CreateFundingTransaction_WithValidRequest_ReturnsCreatedAtActionResult()
        {
            // Arrange
            var request = new CreateFundingRequest
            {
                ProjectId = "project-1",
                Amount = 1000,
                Currency = "USD",
                TransactionType = TransactionType.Contribution
            };

            // Act
            var result = _controller.CreateFundingTransaction(request);

            // Assert
            var createdAtActionResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
            createdAtActionResult.ActionName.Should().Be(nameof(FundingController.GetFundingTransaction));
            var transaction = createdAtActionResult.Value.Should().BeAssignableTo<FundingTransactionDto>().Subject;
            transaction.ProjectId.Should().Be(request.ProjectId);
            transaction.Amount.Should().Be(request.Amount);
            transaction.Currency.Should().Be(request.Currency);
            transaction.TransactionType.Should().Be(request.TransactionType);
            transaction.Status.Should().Be(TransactionStatus.Pending);
        }

        [Fact]
        public void UpdateTransactionStatus_WithValidIdAndRequest_ReturnsOkResult()
        {
            // Arrange
            string transactionId = "1";
            var request = new UpdateTransactionStatusRequest
            {
                Status = TransactionStatus.Completed,
                BlockchainTxId = "0xnewblockchaintxid"
            };

            // Act
            var result = _controller.UpdateTransactionStatus(transactionId, request);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var transaction = okResult.Value.Should().BeAssignableTo<FundingTransactionDto>().Subject;
            transaction.Id.Should().Be(transactionId);
            transaction.Status.Should().Be(request.Status);
            transaction.BlockchainTxId.Should().Be(request.BlockchainTxId);
        }

        [Fact]
        public void CancelTransaction_WithValidId_ReturnsOkResult()
        {
            // Arrange
            string transactionId = "1";

            // Act
            var result = _controller.CancelTransaction(transactionId);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var transaction = okResult.Value.Should().BeAssignableTo<FundingTransactionDto>().Subject;
            transaction.Id.Should().Be(transactionId);
            transaction.Status.Should().Be(TransactionStatus.Cancelled);
        }

        [Fact]
        public void GetFundingSummary_WithValidProjectId_ReturnsOkResult_WithSummary()
        {
            // Arrange
            string projectId = "project-1";

            // Act
            var result = _controller.GetFundingSummary(projectId);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var summary = okResult.Value.Should().BeAssignableTo<FundingSummaryDto>().Subject;
            summary.ProjectId.Should().Be(projectId);
            summary.TotalContributions.Should().BeGreaterThan(0);
        }

        [Fact]
        public void GetFundingByProject_ThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<FundingController>>();
            loggerMock
                .Setup(x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
                .Throws(new Exception("Test exception"));

            var controller = new FundingController(loggerMock.Object);
            string projectId = "error";

            // Act
            var result = controller.GetFundingByProject(projectId);

            // Assert
            var statusCodeResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        }
    }
}
