using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using DecVCPlat.Common.Notifications.New;
using DecVCPlat.Notification.API.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FluentAssertions;

namespace DecVCPlat.Notification.Tests.Controllers
{
    public class NotificationsControllerTests
    {
        private readonly Mock<INotificationService> _notificationServiceMock;
        private readonly Mock<ILogger<NotificationsController>> _loggerMock;
        private readonly NotificationsController _controller;
        private readonly string _userId = "test-user";

        public NotificationsControllerTests()
        {
            _notificationServiceMock = new Mock<INotificationService>();
            _loggerMock = new Mock<ILogger<NotificationsController>>();
            _controller = new NotificationsController(_notificationServiceMock.Object, _loggerMock.Object);

            // Setup controller context with authenticated user
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, _userId),
            }, "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        [Fact]
        public async Task GetNotifications_ReturnsOkResult_WithListOfNotifications()
        {
            // Arrange
            var notifications = new List<Common.Notifications.New.Notification>
            {
                new Common.Notifications.New.Notification
                {
                    Id = "1",
                    UserId = _userId,
                    Title = "Test Notification 1",
                    Message = "This is a test notification",
                    CreatedAt = DateTime.UtcNow.AddDays(-1),
                    IsRead = false
                },
                new Common.Notifications.New.Notification
                {
                    Id = "2",
                    UserId = _userId,
                    Title = "Test Notification 2",
                    Message = "This is another test notification",
                    CreatedAt = DateTime.UtcNow.AddHours(-5),
                    IsRead = true
                }
            };

            _notificationServiceMock
                .Setup(x => x.GetUserNotificationsAsync(_userId, 0, 20, false))
                .ReturnsAsync(notifications);

            // Act
            var result = await _controller.GetNotifications();

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedNotifications = okResult.Value.Should().BeAssignableTo<IEnumerable<Common.Notifications.New.Notification>>().Subject;
            returnedNotifications.Should().BeEquivalentTo(notifications);
        }

        [Fact]
        public async Task GetNotifications_WithPagination_ReturnsOkResult_WithPaginatedNotifications()
        {
            // Arrange
            int skip = 10;
            int take = 5;
            bool includeRead = true;

            var notifications = new List<Common.Notifications.New.Notification>
            {
                new Common.Notifications.New.Notification
                {
                    Id = "3",
                    UserId = _userId,
                    Title = "Test Notification 3",
                    Message = "This is a paginated notification",
                    CreatedAt = DateTime.UtcNow.AddDays(-2),
                    IsRead = true
                }
            };

            _notificationServiceMock
                .Setup(x => x.GetUserNotificationsAsync(_userId, skip, take, includeRead))
                .ReturnsAsync(notifications);

            // Act
            var result = await _controller.GetNotifications(skip, take, includeRead);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedNotifications = okResult.Value.Should().BeAssignableTo<IEnumerable<Common.Notifications.New.Notification>>().Subject;
            returnedNotifications.Should().BeEquivalentTo(notifications);
        }

        [Fact]
        public async Task GetUnreadCount_ReturnsOkResult_WithCount()
        {
            // Arrange
            int unreadCount = 5;

            _notificationServiceMock
                .Setup(x => x.GetUnreadNotificationCountAsync(_userId))
                .ReturnsAsync(unreadCount);

            // Act
            var result = await _controller.GetUnreadCount();

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var count = (int)okResult.Value;
            count.Should().Be(unreadCount);
        }

        [Fact]
        public async Task MarkAsRead_WithValidId_ReturnsNoContentResult()
        {
            // Arrange
            string notificationId = "1";

            _notificationServiceMock
                .Setup(x => x.MarkNotificationAsReadAsync(notificationId, _userId))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.MarkAsRead(notificationId);

            // Assert
            result.Should().BeOfType<NoContentResult>();
        }

        [Fact]
        public async Task MarkAsRead_WithInvalidId_ReturnsNotFoundResult()
        {
            // Arrange
            string notificationId = "999"; // Non-existent ID

            _notificationServiceMock
                .Setup(x => x.MarkNotificationAsReadAsync(notificationId, _userId))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.MarkAsRead(notificationId);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task MarkAllAsRead_ReturnsNoContentResult()
        {
            // Arrange
            _notificationServiceMock
                .Setup(x => x.MarkAllNotificationsAsReadAsync(_userId))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.MarkAllAsRead();

            // Assert
            result.Should().BeOfType<NoContentResult>();
        }

        [Fact]
        public async Task CreateNotification_WithValidRequest_ReturnsCreatedAtActionResult()
        {
            // Arrange
            var request = new CreateNotificationRequest
            {
                UserId = "target-user",
                Title = "New Notification",
                Message = "This is a new notification",
                NotificationType = NotificationType.ProjectUpdate,
                ReferenceId = "project-1"
            };

            var createdNotification = new Common.Notifications.New.Notification
            {
                Id = "new-id",
                UserId = request.UserId,
                Title = request.Title,
                Message = request.Message,
                NotificationType = request.NotificationType,
                ReferenceId = request.ReferenceId,
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };

            _notificationServiceMock
                .Setup(x => x.CreateNotificationAsync(
                    It.Is<Common.Notifications.New.Notification>(n => 
                        n.UserId == request.UserId && 
                        n.Title == request.Title && 
                        n.Message == request.Message)))
                .ReturnsAsync(createdNotification);

            // Act
            var result = await _controller.CreateNotification(request);

            // Assert
            var createdAtActionResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
            createdAtActionResult.ActionName.Should().Be(nameof(NotificationsController.GetNotification));
            var notification = createdAtActionResult.Value.Should().BeAssignableTo<Common.Notifications.New.Notification>().Subject;
            notification.Should().BeEquivalentTo(createdNotification);
        }

        [Fact]
        public async Task GetNotification_WithValidId_ReturnsOkResult_WithNotification()
        {
            // Arrange
            string notificationId = "1";
            var notification = new Common.Notifications.New.Notification
            {
                Id = notificationId,
                UserId = _userId,
                Title = "Test Notification",
                Message = "This is a test notification",
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                IsRead = false
            };

            _notificationServiceMock
                .Setup(x => x.GetNotificationAsync(notificationId, _userId))
                .ReturnsAsync(notification);

            // Act
            var result = await _controller.GetNotification(notificationId);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedNotification = okResult.Value.Should().BeAssignableTo<Common.Notifications.New.Notification>().Subject;
            returnedNotification.Should().BeEquivalentTo(notification);
        }

        [Fact]
        public async Task GetNotification_WithInvalidId_ReturnsNotFoundResult()
        {
            // Arrange
            string notificationId = "999"; // Non-existent ID

            _notificationServiceMock
                .Setup(x => x.GetNotificationAsync(notificationId, _userId))
                .ReturnsAsync((Common.Notifications.New.Notification)null);

            // Act
            var result = await _controller.GetNotification(notificationId);

            // Assert
            result.Result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task DeleteNotification_WithValidId_ReturnsNoContentResult()
        {
            // Arrange
            string notificationId = "1";

            _notificationServiceMock
                .Setup(x => x.DeleteNotificationAsync(notificationId, _userId))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteNotification(notificationId);

            // Assert
            result.Should().BeOfType<NoContentResult>();
        }

        [Fact]
        public async Task DeleteNotification_WithInvalidId_ReturnsNotFoundResult()
        {
            // Arrange
            string notificationId = "999"; // Non-existent ID

            _notificationServiceMock
                .Setup(x => x.DeleteNotificationAsync(notificationId, _userId))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.DeleteNotification(notificationId);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }
    }
}
