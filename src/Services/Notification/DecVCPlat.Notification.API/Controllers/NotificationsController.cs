using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using DecVCPlat.Common.Notifications.New;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DecVCPlat.Notification.API.Controllers
{
    /// <summary>
    /// API controller for managing user notifications
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly ILogger<NotificationsController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationsController"/> class
        /// </summary>
        /// <param name="notificationService">The notification service</param>
        /// <param name="logger">The logger</param>
        public NotificationsController(
            INotificationService notificationService,
            ILogger<NotificationsController> logger)
        {
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets notifications for the authenticated user
        /// </summary>
        /// <param name="skip">Number of notifications to skip</param>
        /// <param name="take">Number of notifications to take</param>
        /// <param name="includeRead">Whether to include read notifications</param>
        /// <returns>A list of notifications</returns>
        /// <response code="200">Returns the list of notifications</response>
        /// <response code="401">If the user is not authenticated</response>
        [HttpGet]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<IEnumerable<Notification>>> GetNotifications(
            [FromQuery] int skip = 0,
            [FromQuery] int take = 20,
            [FromQuery] bool includeRead = false)
        {
            try
            {
                var userId = User.Identity.Name;
                var notifications = await _notificationService.GetUserNotificationsAsync(userId, skip, take, includeRead);
                return Ok(notifications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notifications");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error getting notifications");
            }
        }

        /// <summary>
        /// Gets the count of unread notifications for the authenticated user
        /// </summary>
        /// <returns>The count of unread notifications</returns>
        /// <response code="200">Returns the count of unread notifications</response>
        /// <response code="401">If the user is not authenticated</response>
        [HttpGet("unread-count")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<int>> GetUnreadCount()
        {
            try
            {
                var userId = User.Identity.Name;
                var count = await _notificationService.GetUnreadNotificationCountAsync(userId);
                return Ok(count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting unread notification count");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error getting unread notification count");
            }
        }

        /// <summary>
        /// Marks a notification as read
        /// </summary>
        /// <param name="id">The notification ID</param>
        /// <returns>No content if successful</returns>
        /// <response code="204">If the notification was marked as read</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="404">If the notification was not found</response>
        [HttpPut("{id}/read")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> MarkAsRead(string id)
        {
            try
            {
                var userId = User.Identity.Name;
                var result = await _notificationService.MarkNotificationAsReadAsync(id, userId);
                
                if (!result)
                {
                    return NotFound();
                }
                
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking notification as read");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error marking notification as read");
            }
        }

        /// <summary>
        /// Marks all notifications as read for the authenticated user
        /// </summary>
        /// <returns>The number of notifications marked as read</returns>
        /// <response code="200">Returns the number of notifications marked as read</response>
        /// <response code="401">If the user is not authenticated</response>
        [HttpPut("read-all")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<int>> MarkAllAsRead()
        {
            try
            {
                var userId = User.Identity.Name;
                var count = await _notificationService.MarkAllNotificationsAsReadAsync(userId);
                return Ok(count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking all notifications as read");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error marking all notifications as read");
            }
        }

        /// <summary>
        /// Deletes a notification
        /// </summary>
        /// <param name="id">The notification ID</param>
        /// <returns>No content if successful</returns>
        /// <response code="204">If the notification was deleted</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="404">If the notification was not found</response>
        [HttpDelete("{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteNotification(string id)
        {
            try
            {
                var userId = User.Identity.Name;
                var result = await _notificationService.DeleteNotificationAsync(id, userId);
                
                if (!result)
                {
                    return NotFound();
                }
                
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting notification");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error deleting notification");
            }
        }

        /// <summary>
        /// Deletes all notifications for the authenticated user
        /// </summary>
        /// <returns>The number of notifications deleted</returns>
        /// <response code="200">Returns the number of notifications deleted</response>
        /// <response code="401">If the user is not authenticated</response>
        [HttpDelete]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<int>> DeleteAllNotifications()
        {
            try
            {
                var userId = User.Identity.Name;
                var count = await _notificationService.DeleteAllNotificationsAsync(userId);
                return Ok(count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting all notifications");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error deleting all notifications");
            }
        }

        /// <summary>
        /// Creates a notification for a user (admin only)
        /// </summary>
        /// <param name="request">The notification creation request</param>
        /// <returns>The created notification ID</returns>
        /// <response code="201">Returns the created notification ID</response>
        /// <response code="400">If the request is invalid</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="403">If the user is not authorized</response>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<string>> CreateNotification([FromBody] CreateNotificationRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.UserId))
                {
                    return BadRequest("UserId is required");
                }
                
                var notificationId = await _notificationService.SendNotificationAsync(
                    request.UserId,
                    request.Title,
                    request.Message,
                    request.Type,
                    request.Data);
                
                return CreatedAtAction(nameof(GetNotifications), new { id = notificationId }, notificationId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating notification");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error creating notification");
            }
        }
    }

    /// <summary>
    /// Request model for creating a notification
    /// </summary>
    public class CreateNotificationRequest
    {
        /// <summary>
        /// Gets or sets the user ID to send the notification to
        /// </summary>
        [Required]
        public string UserId { get; set; }
        
        /// <summary>
        /// Gets or sets the notification title
        /// </summary>
        [Required]
        [StringLength(100)]
        public string Title { get; set; }
        
        /// <summary>
        /// Gets or sets the notification message
        /// </summary>
        [Required]
        [StringLength(1000)]
        public string Message { get; set; }
        
        /// <summary>
        /// Gets or sets the notification type
        /// </summary>
        [Required]
        public NotificationType Type { get; set; }
        
        /// <summary>
        /// Gets or sets additional data for the notification
        /// </summary>
        public Dictionary<string, string> Data { get; set; }
    }
}
