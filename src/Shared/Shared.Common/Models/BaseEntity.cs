// © 2024 DecVCPlat. All rights reserved.

using System.ComponentModel.DataAnnotations;

namespace Shared.Common.Models;

public abstract class BaseEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // GDPR and EU AI Act Compliance
    public DateTime? DataRetentionDate { get; set; }

    public bool IsDataProcessingConsented { get; set; } = false;

    public DateTime? ConsentGivenAt { get; set; }

    public DateTime? ConsentWithdrawnAt { get; set; }

    // Audit trail for DecVCPlat compliance
    public string? CreatedBy { get; set; }

    public string? LastModifiedBy { get; set; }

    public bool IsDeleted { get; set; } = false;

    public DateTime? DeletedAt { get; set; }

    public string? DeletionReason { get; set; }
}
