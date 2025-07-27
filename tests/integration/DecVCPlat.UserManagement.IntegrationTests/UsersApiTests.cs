using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using DecVCPlat.UserManagement.API.Controllers;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using FluentAssertions;

namespace DecVCPlat.UserManagement.IntegrationTests
{
    public class UsersApiTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public UsersApiTests(WebApplicationFactory<Program> factory)
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
        public async Task GetUsers_ReturnsSuccessStatusCode()
        {
            // Arrange
            // Add authentication token if needed
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "test-token");

            // Act
            var response = await _client.GetAsync("/api/users");

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var users = JsonSerializer.Deserialize<List<UserDto>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            users.Should().NotBeNull();
        }

        [Fact]
        public async Task GetUser_WithValidId_ReturnsUser()
        {
            // Arrange
            string userId = "1";
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "test-token");

            // Act
            var response = await _client.GetAsync($"/api/users/{userId}");

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var user = JsonSerializer.Deserialize<UserDto>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            user.Should().NotBeNull();
            user.Id.Should().Be(userId);
        }

        [Fact]
        public async Task CreateUser_WithValidData_ReturnsCreatedResponse()
        {
            // Arrange
            var newUser = new CreateUserRequest
            {
                Username = "integrationtest",
                Email = "integration@test.com",
                Password = "Integration123!"
            };
            var content = new StringContent(
                JsonSerializer.Serialize(newUser),
                Encoding.UTF8,
                "application/json");

            // Act
            var response = await _client.PostAsync("/api/users", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            response.Headers.Location.Should().NotBeNull();
            var responseContent = await response.Content.ReadAsStringAsync();
            var createdUser = JsonSerializer.Deserialize<UserDto>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            createdUser.Should().NotBeNull();
            createdUser.Username.Should().Be(newUser.Username);
            createdUser.Email.Should().Be(newUser.Email);
        }

        [Fact]
        public async Task UpdateUser_WithValidData_ReturnsOkResponse()
        {
            // Arrange
            string userId = "1";
            var updateData = new UpdateUserRequest
            {
                Email = "updated-integration@test.com",
                IsActive = false
            };
            var content = new StringContent(
                JsonSerializer.Serialize(updateData),
                Encoding.UTF8,
                "application/json");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "test-token");

            // Act
            var response = await _client.PutAsync($"/api/users/{userId}", content);

            // Assert
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            var updatedUser = JsonSerializer.Deserialize<UserDto>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            updatedUser.Should().NotBeNull();
            updatedUser.Id.Should().Be(userId);
            updatedUser.Email.Should().Be(updateData.Email);
            updatedUser.IsActive.Should().Be(updateData.IsActive);
        }

        [Fact]
        public async Task DeleteUser_WithValidId_ReturnsNoContentResponse()
        {
            // Arrange
            string userId = "1";
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "test-token");

            // Act
            var response = await _client.DeleteAsync($"/api/users/{userId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }
    }
}
