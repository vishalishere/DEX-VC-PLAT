using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using DecVCPlat.Common.Notifications.New;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using FluentAssertions;

namespace DecVCPlat.Notification.IntegrationTests
{
    public class NotificationsApiTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;
        private readonly string _userId = "test-user";

        public NotificationsApiTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    // Configure authentication for testing
                    services.AddAuthentication("Test")
                        .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", options => { });
                });
            });
            _client = _factory.CreateClient();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");
        }

        [Fact]
        public async Task GetNotifications_ReturnsSuccessStatusCode()
        {
            // Act
            var response = await _client.GetAsync("/api/notifications");

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var notifications = JsonSerializer.Deserialize<List<Common.Notifications.New.Notification>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            notifications.Should().NotBeNull();
        }

        [Fact]
        public async Task GetNotifications_WithPagination_ReturnsSuccessStatusCode()
        {
            // Arrange
            int skip = 10;
            int take = 5;
            bool includeRead = true;

            // Act
            var response = await _client.GetAsync($"/api/notifications?skip={skip}&take={take}&includeRead={includeRead}");

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var notifications = JsonSerializer.Deserialize<List<Common.Notifications.New.Notification>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            notifications.Should().NotBeNull();
        }

        [Fact]
        public async Task GetUnreadCount_ReturnsSuccessStatusCode()
        {
            // Act
            var response = await _client.GetAsync("/api/notifications/unread-count");

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var count = JsonSerializer.Deserialize<int>(content);
            count.Should().BeGreaterThanOrEqualTo(0);
        }

        [Fact]
        public async Task CreateNotification_WithValidData_ReturnsCreatedResponse()
        {
            // Arrange
            var newNotification = new CreateNotificationRequest
            {
                UserId = _userId,
                Title = "Integration Test Notification",
                Message = "This is a test notification created during integration testing",
                NotificationType = NotificationType.ProjectUpdate,
                ReferenceId = "project-1"
            };
            var content = new StringContent(
                JsonSerializer.Serialize(newNotification),
                Encoding.UTF8,
                "application/json");

            // Act
            var response = await _client.PostAsync("/api/notifications", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            response.Headers.Location.Should().NotBeNull();
            var responseContent = await response.Content.ReadAsStringAsync();
            var createdNotification = JsonSerializer.Deserialize<Common.Notifications.New.Notification>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            createdNotification.Should().NotBeNull();
            createdNotification.Title.Should().Be(newNotification.Title);
            createdNotification.Message.Should().Be(newNotification.Message);
            createdNotification.NotificationType.Should().Be(newNotification.NotificationType);
            createdNotification.ReferenceId.Should().Be(newNotification.ReferenceId);
        }

        [Fact]
        public async Task GetNotification_WithValidId_ReturnsNotification()
        {
            // Arrange - Create a notification first
            var newNotification = new CreateNotificationRequest
            {
                UserId = _userId,
                Title = "Test Notification for Get",
                Message = "This notification will be retrieved",
                NotificationType = NotificationType.ProjectUpdate,
                ReferenceId = "project-1"
            };
            var createContent = new StringContent(
                JsonSerializer.Serialize(newNotification),
                Encoding.UTF8,
                "application/json");
            var createResponse = await _client.PostAsync("/api/notifications", createContent);
            createResponse.EnsureSuccessStatusCode();
            var createResponseContent = await createResponse.Content.ReadAsStringAsync();
            var createdNotification = JsonSerializer.Deserialize<Common.Notifications.New.Notification>(createResponseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            // Act
            var response = await _client.GetAsync($"/api/notifications/{createdNotification.Id}");

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var notification = JsonSerializer.Deserialize<Common.Notifications.New.Notification>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            notification.Should().NotBeNull();
            notification.Id.Should().Be(createdNotification.Id);
            notification.Title.Should().Be(newNotification.Title);
        }

        [Fact]
        public async Task MarkAsRead_WithValidId_ReturnsNoContent()
        {
            // Arrange - Create a notification first
            var newNotification = new CreateNotificationRequest
            {
                UserId = _userId,
                Title = "Test Notification for Mark as Read",
                Message = "This notification will be marked as read",
                NotificationType = NotificationType.ProjectUpdate,
                ReferenceId = "project-1"
            };
            var createContent = new StringContent(
                JsonSerializer.Serialize(newNotification),
                Encoding.UTF8,
                "application/json");
            var createResponse = await _client.PostAsync("/api/notifications", createContent);
            createResponse.EnsureSuccessStatusCode();
            var createResponseContent = await createResponse.Content.ReadAsStringAsync();
            var createdNotification = JsonSerializer.Deserialize<Common.Notifications.New.Notification>(createResponseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            // Act
            var response = await _client.PutAsync($"/api/notifications/{createdNotification.Id}/read", null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task MarkAllAsRead_ReturnsNoContent()
        {
            // Act
            var response = await _client.PutAsync("/api/notifications/read-all", null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task DeleteNotification_WithValidId_ReturnsNoContent()
        {
            // Arrange - Create a notification first
            var newNotification = new CreateNotificationRequest
            {
                UserId = _userId,
                Title = "Test Notification for Delete",
                Message = "This notification will be deleted",
                NotificationType = NotificationType.ProjectUpdate,
                ReferenceId = "project-1"
            };
            var createContent = new StringContent(
                JsonSerializer.Serialize(newNotification),
                Encoding.UTF8,
                "application/json");
            var createResponse = await _client.PostAsync("/api/notifications", createContent);
            createResponse.EnsureSuccessStatusCode();
            var createResponseContent = await createResponse.Content.ReadAsStringAsync();
            var createdNotification = JsonSerializer.Deserialize<Common.Notifications.New.Notification>(createResponseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            // Act
            var response = await _client.DeleteAsync($"/api/notifications/{createdNotification.Id}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }
    }

    // Test authentication handler for integration tests
    public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public TestAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var claims = new[] { new Claim(ClaimTypes.Name, "test-user") };
            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, "Test");

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}
