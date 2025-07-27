using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using DecVCPlat.Voting.API.Controllers;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using FluentAssertions;

namespace DecVCPlat.Voting.IntegrationTests
{
    public class VotesApiTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public VotesApiTests(WebApplicationFactory<Program> factory)
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
        public async Task GetVotesByProject_ReturnsSuccessStatusCode()
        {
            // Arrange
            string projectId = "project-1";

            // Act
            var response = await _client.GetAsync($"/api/votes/project/{projectId}");

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var votes = JsonSerializer.Deserialize<List<VoteDto>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            votes.Should().NotBeNull();
            votes.Should().HaveCountGreaterThan(0);
        }

        [Fact]
        public async Task GetVotesByProject_WithPagination_ReturnsSuccessStatusCode()
        {
            // Arrange
            string projectId = "project-1";
            int pageNumber = 2;
            int pageSize = 5;

            // Act
            var response = await _client.GetAsync($"/api/votes/project/{projectId}?pageNumber={pageNumber}&pageSize={pageSize}");

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var votes = JsonSerializer.Deserialize<List<VoteDto>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            votes.Should().NotBeNull();
        }

        [Fact]
        public async Task GetVote_WithValidId_ReturnsVote()
        {
            // Arrange
            string voteId = "1";

            // Act
            var response = await _client.GetAsync($"/api/votes/{voteId}");

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var vote = JsonSerializer.Deserialize<VoteDto>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            vote.Should().NotBeNull();
            vote.Id.Should().Be(voteId);
        }

        [Fact]
        public async Task GetVote_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            string voteId = "999"; // Non-existent ID

            // Act
            var response = await _client.GetAsync($"/api/votes/{voteId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task CreateVote_WithValidData_ReturnsCreatedResponse()
        {
            // Arrange
            var newVote = new CreateVoteRequest
            {
                ProjectId = "project-1",
                VoteType = VoteType.Positive,
                Weight = 1,
                Comment = "Integration test vote"
            };
            var content = new StringContent(
                JsonSerializer.Serialize(newVote),
                Encoding.UTF8,
                "application/json");

            // Act
            var response = await _client.PostAsync("/api/votes", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            response.Headers.Location.Should().NotBeNull();
            var responseContent = await response.Content.ReadAsStringAsync();
            var createdVote = JsonSerializer.Deserialize<VoteDto>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            createdVote.Should().NotBeNull();
            createdVote.ProjectId.Should().Be(newVote.ProjectId);
            createdVote.VoteType.Should().Be(newVote.VoteType);
            createdVote.Weight.Should().Be(newVote.Weight);
            createdVote.Comment.Should().Be(newVote.Comment);
        }

        [Fact]
        public async Task UpdateVote_WithValidData_ReturnsOkResponse()
        {
            // Arrange
            string voteId = "1";
            var updateData = new UpdateVoteRequest
            {
                VoteType = VoteType.Neutral,
                Comment = "Updated during integration testing"
            };
            var content = new StringContent(
                JsonSerializer.Serialize(updateData),
                Encoding.UTF8,
                "application/json");

            // Act
            var response = await _client.PutAsync($"/api/votes/{voteId}", content);

            // Assert
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            var updatedVote = JsonSerializer.Deserialize<VoteDto>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            updatedVote.Should().NotBeNull();
            updatedVote.Id.Should().Be(voteId);
            updatedVote.VoteType.Should().Be(updateData.VoteType);
            updatedVote.Comment.Should().Be(updateData.Comment);
        }

        [Fact]
        public async Task DeleteVote_WithValidId_ReturnsNoContentResponse()
        {
            // Arrange
            string voteId = "1";

            // Act
            var response = await _client.DeleteAsync($"/api/votes/{voteId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task GetVotingSummary_WithValidProjectId_ReturnsSummary()
        {
            // Arrange
            string projectId = "project-1";

            // Act
            var response = await _client.GetAsync($"/api/votes/summary/{projectId}");

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var summary = JsonSerializer.Deserialize<VotingSummaryDto>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            summary.Should().NotBeNull();
            summary.ProjectId.Should().Be(projectId);
        }
    }
}
