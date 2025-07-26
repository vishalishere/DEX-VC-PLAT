using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace DecVCPlat.Notification.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        /// <summary>
        /// Retrieves all notifications for the current user
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /api/notification
        ///
        /// </remarks>
        /// <returns>A list of all notifications for the user</returns>
        /// <response code="200">Returns the list of notifications</response>
        /// <response code="401">If the user is not authenticated</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<NotificationDto>), 200)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<IEnumerable<NotificationDto>>> GetNotifications()
        {
            // In a real implementation, this would retrieve notifications from a database
            var notifications = new List<NotificationDto>
            {
                new NotificationDto 
                { 
                    Id = 1, 
                    UserId = 1,
                    Title = "New Vote Required", 
                    Message = "A new voting session has been created for project 'Decentralized Finance Platform'", 
                    Type = "Voting",
                    RelatedEntityId = 1,
                    RelatedEntityType = "VotingSession",
                    IsRead = false,
                    CreatedAt = DateTime.Now.AddHours(-5)
                },
                new NotificationDto 
                { 
                    Id = 2, 
                    UserId = 1,
                    Title = "Funding Milestone Reached", 
                    Message = "Project 'Decentralized Finance Platform' has reached 75% of its funding goal", 
                    Type = "Funding",
                    RelatedEntityId = 1,
                    RelatedEntityType = "Project",
                    IsRead = true,
                    CreatedAt = DateTime.Now.AddDays(-1)
                },
                new NotificationDto 
                { 
                    Id = 3, 
                    UserId = 1,
                    Title = "Project Update", 
                    Message = "Project 'Blockchain Supply Chain' has been updated", 
                    Type = "Project",
                    RelatedEntityId = 2,
                    RelatedEntityType = "Project",
                    IsRead = false,
                    CreatedAt = DateTime.Now.AddDays(-2)
                }
            };

            return Ok(notifications);
        }

        /// <summary>
        /// Retrieves a specific notification by id
        /// </summary>
        /// <param name="id">The notification id</param>
        /// <returns>The notification details</returns>
        /// <response code="200">Returns the notification</response>
        /// <response code="404">If the notification is not found</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="403">If the user is not authorized to view the notification</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(NotificationDto), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<NotificationDto>> GetNotification(int id)
        {
            // In a real implementation, this would retrieve the notification from a database
            if (id < 1 || id > 3)
            {
                return NotFound();
            }

            NotificationDto notification;
            
            switch (id)
            {
                case 1:
                    notification = new NotificationDto 
                    { 
                        Id = 1, 
                        UserId = 1,
                        Title = "New Vote Required", 
                        Message = "A new voting session has been created for project 'Decentralized Finance Platform'", 
                        Type = "Voting",
                        RelatedEntityId = 1,
                        RelatedEntityType = "VotingSession",
                        IsRead = false,
                        CreatedAt = DateTime.Now.AddHours(-5)
                    };
                    break;
                case 2:
                    notification = new NotificationDto 
                    { 
                        Id = 2, 
                        UserId = 1,
                        Title = "Funding Milestone Reached", 
                        Message = "Project 'Decentralized Finance Platform' has reached 75% of its funding goal", 
                        Type = "Funding",
                        RelatedEntityId = 1,
                        RelatedEntityType = "Project",
                        IsRead = true,
                        CreatedAt = DateTime.Now.AddDays(-1)
                    };
                    break;
                default:
                    notification = new NotificationDto 
                    { 
                        Id = 3, 
                        UserId = 1,
                        Title = "Project Update", 
                        Message = "Project 'Blockchain Supply Chain' has been updated", 
                        Type = "Project",
                        RelatedEntityId = 2,
                        RelatedEntityType = "Project",
                        IsRead = false,
                        CreatedAt = DateTime.Now.AddDays(-2)
                    };
                    break;
            }

            return Ok(notification);
        }

        /// <summary>
        /// Creates a new notification
        /// </summary>
        /// <param name="createNotificationDto">The notification creation data</param>
        /// <returns>The newly created notification</returns>
        /// <response code="201">Returns the newly created notification</response>
        /// <response code="400">If the request data is invalid</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="403">If the user is not authorized to create notifications</response>
        [HttpPost]
        [ProducesResponseType(typeof(NotificationDto), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<NotificationDto>> CreateNotification(CreateNotificationDto createNotificationDto)
        {
            // In a real implementation, this would create a new notification in the database
            var newNotification = new NotificationDto
            {
                Id = 4,
                UserId = createNotificationDto.UserId,
                Title = createNotificationDto.Title,
                Message = createNotificationDto.Message,
                Type = createNotificationDto.Type,
                RelatedEntityId = createNotificationDto.RelatedEntityId,
                RelatedEntityType = createNotificationDto.RelatedEntityType,
                IsRead = false,
                CreatedAt = DateTime.Now
            };

            return CreatedAtAction(nameof(GetNotification), new { id = newNotification.Id }, newNotification);
        }

        /// <summary>
        /// Marks a notification as read
        /// </summary>
        /// <param name="id">The notification id</param>
        /// <returns>No content</returns>
        /// <response code="204">If the notification was marked as read successfully</response>
        /// <response code="404">If the notification is not found</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="403">If the user is not authorized to update the notification</response>
        [HttpPut("{id}/read")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            // In a real implementation, this would update the notification in the database
            if (id < 1 || id > 3)
            {
                return NotFound();
            }

            // Update logic would go here

            return NoContent();
        }

        /// <summary>
        /// Marks all notifications as read for the current user
        /// </summary>
        /// <returns>No content</returns>
        /// <response code="204">If the notifications were marked as read successfully</response>
        /// <response code="401">If the user is not authenticated</response>
        [HttpPut("read-all")]
        [ProducesResponseType(204)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> MarkAllAsRead()
        {
            // In a real implementation, this would update all notifications in the database
            // Update logic would go here

            return NoContent();
        }

        /// <summary>
        /// Deletes a specific notification
        /// </summary>
        /// <param name="id">The notification id</param>
        /// <returns>No content</returns>
        /// <response code="204">If the notification was deleted successfully</response>
        /// <response code="404">If the notification is not found</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="403">If the user is not authorized to delete the notification</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> DeleteNotification(int id)
        {
            // In a real implementation, this would delete the notification from the database
            if (id < 1 || id > 3)
            {
                return NotFound();
            }

            // Delete logic would go here

            return NoContent();
        }

        /// <summary>
        /// Gets the count of unread notifications for the current user
        /// </summary>
        /// <returns>The count of unread notifications</returns>
        /// <response code="200">Returns the count of unread notifications</response>
        /// <response code="401">If the user is not authenticated</response>
        [HttpGet("unread-count")]
        [ProducesResponseType(typeof(UnreadCountDto), 200)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<UnreadCountDto>> GetUnreadCount()
        {
            // In a real implementation, this would count unread notifications in the database
            var count = new UnreadCountDto
            {
                UserId = 1,
                UnreadCount = 2
            };

            return Ok(count);
        }

        /// <summary>
        /// Subscribes to notification channels
        /// </summary>
        /// <param name="subscriptionDto">The subscription data</param>
        /// <returns>The updated subscription settings</returns>
        /// <response code="200">Returns the updated subscription settings</response>
        /// <response code="400">If the request data is invalid</response>
        /// <response code="401">If the user is not authenticated</response>
        [HttpPost("subscribe")]
        [ProducesResponseType(typeof(NotificationSubscriptionDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<NotificationSubscriptionDto>> Subscribe(NotificationSubscriptionDto subscriptionDto)
        {
            // In a real implementation, this would update subscription settings in the database
            var subscription = new NotificationSubscriptionDto
            {
                UserId = 1,
                EmailEnabled = subscriptionDto.EmailEnabled,
                PushEnabled = subscriptionDto.PushEnabled,
                InAppEnabled = subscriptionDto.InAppEnabled,
                SubscribedTypes = subscriptionDto.SubscribedTypes
            };

            return Ok(subscription);
        }
    }

    public class NotificationDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string Type { get; set; }
        public int RelatedEntityId { get; set; }
        public string RelatedEntityType { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateNotificationDto
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        [Required]
        [StringLength(1000)]
        public string Message { get; set; }

        [Required]
        [StringLength(50)]
        public string Type { get; set; }

        [Required]
        public int RelatedEntityId { get; set; }

        [Required]
        [StringLength(50)]
        public string RelatedEntityType { get; set; }
    }

    public class UnreadCountDto
    {
        public int UserId { get; set; }
        public int UnreadCount { get; set; }
    }

    public class NotificationSubscriptionDto
    {
        public int UserId { get; set; }
        public bool EmailEnabled { get; set; }
        public bool PushEnabled { get; set; }
        public bool InAppEnabled { get; set; }
        public List<string> SubscribedTypes { get; set; }
    }
}
