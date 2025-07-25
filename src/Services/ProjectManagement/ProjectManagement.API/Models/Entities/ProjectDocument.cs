// © 2024 DecVCPlat. All rights reserved.

using System.ComponentModel.DataAnnotations;

namespace ProjectManagement.API.Models.Entities;

public class ProjectDocument
{
    public Guid Id { get; set; }

    [Required]
    public Guid ProjectId { get; set; }

    [Required]
    [MaxLength(200)]
    public string FileName { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string FilePath { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string ContentType { get; set; } = string.Empty;

    public long FileSize { get; set; }

    public DocumentType DocumentType { get; set; } = DocumentType.Other;

    [MaxLength(500)]
    public string? Description { get; set; }

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    [Required]
    public Guid UploadedByUserId { get; set; }

    [MaxLength(100)]
    public string UploadedByUserName { get; set; } = string.Empty;

    public bool IsPublic { get; set; } = false;

    // GDPR compliance
    public DateTime? ExpiryDate { get; set; }

    public bool IsEncrypted { get; set; } = false;

    [MaxLength(200)]
    public string? EncryptionKeyId { get; set; }

    // Navigation property
    public Project Project { get; set; } = null!;
}

public enum DocumentType
{
    BusinessPlan = 0,
    FinancialProjection = 1,
    TechnicalSpec = 2,
    LegalDocument = 3,
    Whitepaper = 4,
    Presentation = 5,
    Other = 99
}
