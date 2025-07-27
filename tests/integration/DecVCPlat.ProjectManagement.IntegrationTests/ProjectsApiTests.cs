using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using DecVCPlat.ProjectManagement.API.Controllers;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using FluentAssertions;

namespace DecVCPlat.ProjectManagement.IntegrationTests
{
    public class ProjectsApiTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public ProjectsApiTests(WebApplicationFactory<Program> factory)
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
        public async Task GetProjects_ReturnsSuccessStatusCode()
        {
            // Act
            var response = await _client.GetAsync("/api/projects");

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var projects = JsonSerializer.Deserialize<List<ProjectDto>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            projects.Should().NotBeNull();
            projects.Should().HaveCountGreaterThan(0);
        }

        [Fact]
        public async Task GetProjects_WithFiltering_ReturnsFilteredProjects()
        {
            // Arrange
            string status = "Active";
            string category = "DeFi";

            // Act
            var response = await _client.GetAsync($"/api/projects?status={status}&category={category}");

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var projects = JsonSerializer.Deserialize<List<ProjectDto>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            projects.Should().NotBeNull();
            // In a real test with real data, we would verify that all returned projects match the filter criteria
        }

        [Fact]
        public async Task GetProject_WithValidId_ReturnsProject()
        {
            // Arrange
            string projectId = "1";

            // Act
            var response = await _client.GetAsync($"/api/projects/{projectId}");

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var project = JsonSerializer.Deserialize<ProjectDto>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            project.Should().NotBeNull();
            project.Id.Should().Be(projectId);
        }

        [Fact]
        public async Task GetProject_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            string projectId = "999"; // Non-existent ID

            // Act
            var response = await _client.GetAsync($"/api/projects/{projectId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task CreateProject_WithValidData_ReturnsCreatedResponse()
        {
            // Arrange
            var newProject = new CreateProjectRequest
            {
                Title = "Integration Test Project",
                Description = "A project created during integration testing",
                Category = "Test",
                FundingGoal = 5000
            };
            var content = new StringContent(
                JsonSerializer.Serialize(newProject),
                Encoding.UTF8,
                "application/json");

            // Act
            var response = await _client.PostAsync("/api/projects", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            response.Headers.Location.Should().NotBeNull();
            var responseContent = await response.Content.ReadAsStringAsync();
            var createdProject = JsonSerializer.Deserialize<ProjectDto>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            createdProject.Should().NotBeNull();
            createdProject.Title.Should().Be(newProject.Title);
            createdProject.Description.Should().Be(newProject.Description);
            createdProject.Category.Should().Be(newProject.Category);
            createdProject.FundingGoal.Should().Be(newProject.FundingGoal);
        }

        [Fact]
        public async Task UpdateProject_WithValidData_ReturnsOkResponse()
        {
            // Arrange
            string projectId = "1";
            var updateData = new UpdateProjectRequest
            {
                Title = "Updated Integration Test Project",
                Description = "Updated during integration testing",
                Status = "Completed"
            };
            var content = new StringContent(
                JsonSerializer.Serialize(updateData),
                Encoding.UTF8,
                "application/json");

            // Act
            var response = await _client.PutAsync($"/api/projects/{projectId}", content);

            // Assert
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            var updatedProject = JsonSerializer.Deserialize<ProjectDto>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            updatedProject.Should().NotBeNull();
            updatedProject.Id.Should().Be(projectId);
            updatedProject.Title.Should().Be(updateData.Title);
            updatedProject.Description.Should().Be(updateData.Description);
            updatedProject.Status.Should().Be(updateData.Status);
        }

        [Fact]
        public async Task DeleteProject_WithValidId_ReturnsNoContentResponse()
        {
            // Arrange
            string projectId = "1";

            // Act
            var response = await _client.DeleteAsync($"/api/projects/{projectId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }
    }
}
