// © 2024 DecVCPlat. All rights reserved.

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;
using DecVCPlat.ProjectManagement.DTOs;

namespace DecVCPlat.IntegrationTests;

public class DecVCPlatProjectManagementIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public DecVCPlatProjectManagementIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task DecVCPlat_CreateProject_ValidRequest_ReturnsSuccess()
    {
        // Arrange
        var createRequest = new DecVCPlatCreateProjectRequest
        {
            Title = "DecVCPlat Test Project",
            Description = "Integration test project for DecVCPlat platform",
            Category = "Technology",
            FundingGoal = 100000,
            Duration = 12,
            Tags = new List<string> { "blockchain", "fintech", "startup" },
            Milestones = new List<DecVCPlatCreateMilestoneRequest>
            {
                new DecVCPlatCreateMilestoneRequest
                {
                    Title = "MVP Development",
                    Description = "Minimum viable product",
                    DueDate = DateTime.UtcNow.AddMonths(3),
                    FundingPercentage = 50
                }
            }
        };

        // Add authorization header (mock JWT token)
        _client.DefaultRequestHeaders.Add("Authorization", "Bearer mock-jwt-token");

        // Act
        var response = await _client.PostAsJsonAsync("/api/projects", createRequest);

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var project = JsonSerializer.Deserialize<DecVCPlatProjectResponse>(content, new JsonSerializerOptions 
        { 
            PropertyNameCaseInsensitive = true 
        });

        Assert.NotNull(project);
        Assert.Equal(createRequest.Title, project.Title);
        Assert.Equal(createRequest.Description, project.Description);
        Assert.Equal(createRequest.FundingGoal, project.FundingGoal);
    }

    [Fact]
    public async Task DecVCPlat_GetProjects_ReturnsProjectList()
    {
        // Arrange
        _client.DefaultRequestHeaders.Add("Authorization", "Bearer mock-jwt-token");

        // Act
        var response = await _client.GetAsync("/api/projects");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var projects = JsonSerializer.Deserialize<List<DecVCPlatProjectResponse>>(content, new JsonSerializerOptions 
        { 
            PropertyNameCaseInsensitive = true 
        });

        Assert.NotNull(projects);
        Assert.IsType<List<DecVCPlatProjectResponse>>(projects);
    }

    [Fact]
    public async Task DecVCPlat_VoteOnProject_ValidVote_ReturnsSuccess()
    {
        // Arrange
        var projectId = "test-project-id";
        var voteRequest = new DecVCPlatVoteRequest
        {
            Vote = true,
            Comments = "Great project with strong potential"
        };

        _client.DefaultRequestHeaders.Add("Authorization", "Bearer mock-investor-jwt-token");

        // Act
        var response = await _client.PostAsJsonAsync($"/api/projects/{projectId}/vote", voteRequest);

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Vote recorded successfully", content);
    }

    [Fact]
    public async Task DecVCPlat_GetProjectDetails_ValidId_ReturnsProjectDetails()
    {
        // Arrange
        var projectId = "test-project-id";
        _client.DefaultRequestHeaders.Add("Authorization", "Bearer mock-jwt-token");

        // Act
        var response = await _client.GetAsync($"/api/projects/{projectId}");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var project = JsonSerializer.Deserialize<DecVCPlatProjectResponse>(content, new JsonSerializerOptions 
        { 
            PropertyNameCaseInsensitive = true 
        });

        Assert.NotNull(project);
        Assert.Equal(projectId, project.Id);
    }

    [Fact]
    public async Task DecVCPlat_UpdateProjectStatus_ValidStatus_ReturnsSuccess()
    {
        // Arrange
        var projectId = "test-project-id";
        var statusRequest = new DecVCPlatUpdateProjectStatusRequest
        {
            Status = "InReview",
            Comments = "Project submitted for review"
        };

        _client.DefaultRequestHeaders.Add("Authorization", "Bearer mock-founder-jwt-token");

        // Act
        var response = await _client.PutAsJsonAsync($"/api/projects/{projectId}/status", statusRequest);

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Project status updated successfully", content);
    }

    [Fact]
    public async Task DecVCPlat_CreateProject_UnauthorizedUser_ReturnsUnauthorized()
    {
        // Arrange
        var createRequest = new DecVCPlatCreateProjectRequest
        {
            Title = "Unauthorized Test Project",
            Description = "This should fail",
            FundingGoal = 50000
        };

        // No authorization header

        // Act
        var response = await _client.PostAsJsonAsync("/api/projects", createRequest);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task DecVCPlat_GetProjectsByCategory_ValidCategory_ReturnsFilteredProjects()
    {
        // Arrange
        var category = "Technology";
        _client.DefaultRequestHeaders.Add("Authorization", "Bearer mock-jwt-token");

        // Act
        var response = await _client.GetAsync($"/api/projects?category={category}");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var projects = JsonSerializer.Deserialize<List<DecVCPlatProjectResponse>>(content, new JsonSerializerOptions 
        { 
            PropertyNameCaseInsensitive = true 
        });

        Assert.NotNull(projects);
        Assert.All(projects, p => Assert.Equal(category, p.Category));
    }

    [Fact]
    public async Task DecVCPlat_SearchProjects_ValidQuery_ReturnsSearchResults()
    {
        // Arrange
        var searchQuery = "blockchain";
        _client.DefaultRequestHeaders.Add("Authorization", "Bearer mock-jwt-token");

        // Act
        var response = await _client.GetAsync($"/api/projects/search?query={searchQuery}");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var projects = JsonSerializer.Deserialize<List<DecVCPlatProjectResponse>>(content, new JsonSerializerOptions 
        { 
            PropertyNameCaseInsensitive = true 
        });

        Assert.NotNull(projects);
        Assert.All(projects, p => 
            Assert.True(p.Title.Contains(searchQuery, StringComparison.OrdinalIgnoreCase) ||
                       p.Description.Contains(searchQuery, StringComparison.OrdinalIgnoreCase) ||
                       p.Tags.Any(t => t.Contains(searchQuery, StringComparison.OrdinalIgnoreCase))));
    }
}

// DTOs for integration tests
public class DecVCPlatCreateProjectRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal FundingGoal { get; set; }
    public int Duration { get; set; }
    public List<string> Tags { get; set; } = new();
    public List<DecVCPlatCreateMilestoneRequest> Milestones { get; set; } = new();
}

public class DecVCPlatCreateMilestoneRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public decimal FundingPercentage { get; set; }
}

public class DecVCPlatProjectResponse
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal FundingGoal { get; set; }
    public decimal CurrentFunding { get; set; }
    public List<string> Tags { get; set; } = new();
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class DecVCPlatVoteRequest
{
    public bool Vote { get; set; }
    public string Comments { get; set; } = string.Empty;
}

public class DecVCPlatUpdateProjectStatusRequest
{
    public string Status { get; set; } = string.Empty;
    public string Comments { get; set; } = string.Empty;
}
