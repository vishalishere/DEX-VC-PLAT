// © 2024 DecVCPlat. All rights reserved.

using System.ComponentModel.DataAnnotations;

namespace ProjectManagement.API.Models.Entities;

public class ProjectComment
{
    public Guid Id { get; set; }

    [Required]
    public Guid ProjectId { get; set; }

    [Required]
    public Guid AuthorId { get; set; }

    [MaxLength(100)]
    public string AuthorName { get; set; } = string.Empty;

    [Required]
    [MaxLength(2000)]
    public string Content { get; set; } = string.Empty;

    public CommentType CommentType { get; set; } = CommentType.General;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public bool IsEdited { get; set; } = false;

    // For threading/replies
    public Guid? ParentCommentId { get; set; }

    public ProjectComment? ParentComment { get; set; }

    public ICollection<ProjectComment> Replies { get; set; } = new List<ProjectComment>();

    // Moderation
    public bool IsDeleted { get; set; } = false;

    public bool IsFlagged { get; set; } = false;

    [MaxLength(500)]
    public string? ModerationReason { get; set; }

    // Navigation property
    public Project Project { get; set; } = null!;
}

public enum CommentType
{
    General = 0,
    Question = 1,
    Feedback = 2,
    Concern = 3,
    Support = 4,
    TechnicalReview = 5,
    FinancialReview = 6
}
