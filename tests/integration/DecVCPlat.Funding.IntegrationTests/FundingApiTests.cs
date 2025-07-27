using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using DecVCPlat.Funding.API.Controllers;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using FluentAssertions;

namespace DecVCPlat.Funding.IntegrationTests
{
    public class FundingApiTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public FundingApiTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Configure test services if needed
                });
            });
            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task GetFundingByProject_ReturnsSuccessStatusCode()
        {
            // Arrange
            string projectId = "project-1";

            // Act
            var response = await _client.GetAsync($"/api/funding/project/{projectId}");

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var transactions = JsonSerializer.Deserialize<List<FundingTransactionDto>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            transactions.Should().NotBeNull();
            transactions.Should().HaveCountGreaterThan(0);
        }

        [Fact]
        public async Task GetFundingByProject_WithPagination_ReturnsSuccessStatusCode()
        {
            // Arrange
            string projectId = "project-1";
            int pageNumber = 2;
            int pageSize = 5;

            // Act
            var response = await _client.GetAsync($"/api/funding/project/{projectId}?pageNumber={pageNumber}&pageSize={pageSize}");

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var transactions = JsonSerializer.Deserialize<List<FundingTransactionDto>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            transactions.Should().NotBeNull();
        }

        [Fact]
        public async Task GetFundingTransaction_WithValidId_ReturnsTransaction()
        {
            // Arrange
            string transactionId = "1";

            // Act
            var response = await _client.GetAsync($"/api/funding/{transactionId}");

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var transaction = JsonSerializer.Deserialize<FundingTransactionDto>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            transaction.Should().NotBeNull();
            transaction.Id.Should().Be(transactionId);
        }

        [Fact]
        public async Task GetFundingTransaction_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            string transactionId = "999"; // Non-existent ID

            // Act
            var response = await _client.GetAsync($"/api/funding/{transactionId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task CreateFundingTransaction_WithValidData_ReturnsCreatedResponse()
        {
            // Arrange
            var newTransaction = new CreateFundingRequest
            {
                ProjectId = "project-1",
                Amount = 1000,
                Currency = "USD",
                TransactionType = TransactionType.Contribution
            };
            var content = new StringContent(
                JsonSerializer.Serialize(newTransaction),
                Encoding.UTF8,
                "application/json");

            // Act
            var response = await _client.PostAsync("/api/funding", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            response.Headers.Location.Should().NotBeNull();
            var responseContent = await response.Content.ReadAsStringAsync();
            var createdTransaction = JsonSerializer.Deserialize<FundingTransactionDto>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            createdTransaction.Should().NotBeNull();
            createdTransaction.ProjectId.Should().Be(newTransaction.ProjectId);
            createdTransaction.Amount.Should().Be(newTransaction.Amount);
            createdTransaction.Currency.Should().Be(newTransaction.Currency);
            createdTransaction.TransactionType.Should().Be(newTransaction.TransactionType);
            createdTransaction.Status.Should().Be(TransactionStatus.Pending);
        }

        [Fact]
        public async Task UpdateTransactionStatus_WithValidData_ReturnsOkResponse()
        {
            // Arrange
            string transactionId = "1";
            var updateData = new UpdateTransactionStatusRequest
            {
                Status = TransactionStatus.Completed,
                BlockchainTxId = "0xnewblockchaintxid"
            };
            var content = new StringContent(
                JsonSerializer.Serialize(updateData),
                Encoding.UTF8,
                "application/json");

            // Act
            var response = await _client.PutAsync($"/api/funding/{transactionId}/status", content);

            // Assert
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            var updatedTransaction = JsonSerializer.Deserialize<FundingTransactionDto>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            updatedTransaction.Should().NotBeNull();
            updatedTransaction.Id.Should().Be(transactionId);
            updatedTransaction.Status.Should().Be(updateData.Status);
            updatedTransaction.BlockchainTxId.Should().Be(updateData.BlockchainTxId);
        }

        [Fact]
        public async Task CancelTransaction_WithValidId_ReturnsOkResponse()
        {
            // Arrange
            string transactionId = "1";

            // Act
            var response = await _client.PutAsync($"/api/funding/{transactionId}/cancel", null);

            // Assert
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            var cancelledTransaction = JsonSerializer.Deserialize<FundingTransactionDto>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            cancelledTransaction.Should().NotBeNull();
            cancelledTransaction.Id.Should().Be(transactionId);
            cancelledTransaction.Status.Should().Be(TransactionStatus.Cancelled);
        }

        [Fact]
        public async Task GetFundingSummary_WithValidProjectId_ReturnsSummary()
        {
            // Arrange
            string projectId = "project-1";

            // Act
            var response = await _client.GetAsync($"/api/funding/summary/{projectId}");

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var summary = JsonSerializer.Deserialize<FundingSummaryDto>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            summary.Should().NotBeNull();
            summary.ProjectId.Should().Be(projectId);
            summary.TotalContributions.Should().BeGreaterThan(0);
        }
    }
}
