// © 2024 DecVCPlat. All rights reserved.

using Microsoft.EntityFrameworkCore;
using NotificationService.API.Models.Entities;

namespace NotificationService.API.Data;

public class NotificationDbContext : DbContext
{
    public NotificationDbContext(DbContextOptions<NotificationDbContext> options) : base(options)
    {
    }

    public DbSet<Notification> Notifications { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Notification entity
        modelBuilder.Entity<Notification>(entity =>
        {
            entity.ToTable("DecVCPlat_Notifications");
            entity.HasKey(n => n.Id);
            
            entity.Property(n => n.Title).IsRequired().HasMaxLength(200);
            entity.Property(n => n.Message).IsRequired().HasMaxLength(1000);
            entity.Property(n => n.UserName).HasMaxLength(100);
            entity.Property(n => n.Type).HasConversion<string>();
            entity.Property(n => n.Priority).HasConversion<string>();
            entity.Property(n => n.Status).HasConversion<string>();
            
            entity.Property(n => n.EmailSubject).HasMaxLength(200);
            entity.Property(n => n.EmailBody).HasMaxLength(5000);
            entity.Property(n => n.EmailError).HasMaxLength(500);
            entity.Property(n => n.PushPayload).HasMaxLength(1000);
            entity.Property(n => n.PushError).HasMaxLength(500);
            entity.Property(n => n.ActionUrl).HasMaxLength(200);
            entity.Property(n => n.ActionText).HasMaxLength(100);
            entity.Property(n => n.MetadataJson).HasMaxLength(2000);
            
            // Indexes for performance
            entity.HasIndex(n => n.UserId);
            entity.HasIndex(n => n.Type);
            entity.HasIndex(n => n.Status);
            entity.HasIndex(n => n.Priority);
            entity.HasIndex(n => n.CreatedAt);
            entity.HasIndex(n => n.ProjectId);
            entity.HasIndex(n => n.ProposalId);
            entity.HasIndex(n => n.BatchId);
            
            // Composite indexes for common queries
            entity.HasIndex(n => new { n.UserId, n.Status });
            entity.HasIndex(n => new { n.UserId, n.CreatedAt });
            entity.HasIndex(n => new { n.Type, n.Status });
        });
    }
}
