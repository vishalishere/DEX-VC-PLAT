// © 2024 DecVCPlat. All rights reserved.

using Microsoft.EntityFrameworkCore;
using ProjectManagement.API.Models.Entities;

namespace ProjectManagement.API.Data;

public class ProjectDbContext : DbContext
{
    public ProjectDbContext(DbContextOptions<ProjectDbContext> options) : base(options)
    {
    }

    public DbSet<Project> Projects { get; set; }
    public DbSet<ProjectMilestone> ProjectMilestones { get; set; }
    public DbSet<ProjectVote> ProjectVotes { get; set; }
    public DbSet<ProjectDocument> ProjectDocuments { get; set; }
    public DbSet<ProjectComment> ProjectComments { get; set; }
    public DbSet<ProjectFunding> ProjectFundings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Project entity
        modelBuilder.Entity<Project>(entity =>
        {
            entity.ToTable("DecVCPlat_Projects");
            entity.HasKey(p => p.Id);
            
            entity.Property(p => p.Title).IsRequired().HasMaxLength(200);
            entity.Property(p => p.Description).IsRequired().HasMaxLength(5000);
            entity.Property(p => p.FounderId).IsRequired();
            entity.Property(p => p.FounderName).HasMaxLength(100);
            entity.Property(p => p.Category).IsRequired().HasMaxLength(100);
            
            entity.Property(p => p.FundingGoal).HasPrecision(18, 2);
            entity.Property(p => p.CurrentFunding).HasPrecision(18, 2);
            
            entity.Property(p => p.Status).HasConversion<string>();
            entity.Property(p => p.RiskLevel).HasConversion<string>();
            
            entity.HasIndex(p => p.FounderId);
            entity.HasIndex(p => p.Category);
            entity.HasIndex(p => p.Status);
            entity.HasIndex(p => p.CreatedAt);
        });

        // Configure ProjectMilestone entity
        modelBuilder.Entity<ProjectMilestone>(entity =>
        {
            entity.ToTable("DecVCPlat_ProjectMilestones");
            entity.HasKey(m => m.Id);
            
            entity.Property(m => m.Title).IsRequired().HasMaxLength(200);
            entity.Property(m => m.Description).HasMaxLength(2000);
            entity.Property(m => m.FundingAmount).HasPrecision(18, 2);
            entity.Property(m => m.Status).HasConversion<string>();
            
            entity.HasOne(m => m.Project)
                  .WithMany(p => p.Milestones)
                  .HasForeignKey(m => m.ProjectId)
                  .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasIndex(m => m.ProjectId);
            entity.HasIndex(m => m.Status);
            entity.HasIndex(m => m.DueDate);
        });

        // Configure ProjectVote entity
        modelBuilder.Entity<ProjectVote>(entity =>
        {
            entity.ToTable("DecVCPlat_ProjectVotes");
            entity.HasKey(v => v.Id);
            
            entity.Property(v => v.StakedTokens).HasPrecision(18, 8);
            entity.Property(v => v.VotingPower).HasPrecision(18, 8);
            entity.Property(v => v.VoteType).HasConversion<int>();
            
            entity.HasOne(v => v.Project)
                  .WithMany(p => p.Votes)
                  .HasForeignKey(v => v.ProjectId)
                  .OnDelete(DeleteBehavior.Cascade);
            
            // Unique constraint: one vote per voter per project
            entity.HasIndex(v => new { v.ProjectId, v.VoterId }).IsUnique();
            entity.HasIndex(v => v.VotedAt);
        });

        // Configure ProjectDocument entity
        modelBuilder.Entity<ProjectDocument>(entity =>
        {
            entity.ToTable("DecVCPlat_ProjectDocuments");
            entity.HasKey(d => d.Id);
            
            entity.Property(d => d.FileName).IsRequired().HasMaxLength(200);
            entity.Property(d => d.FilePath).IsRequired().HasMaxLength(500);
            entity.Property(d => d.ContentType).IsRequired().HasMaxLength(100);
            entity.Property(d => d.DocumentType).HasConversion<string>();
            
            entity.HasOne(d => d.Project)
                  .WithMany(p => p.Documents)
                  .HasForeignKey(d => d.ProjectId)
                  .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasIndex(d => d.ProjectId);
            entity.HasIndex(d => d.DocumentType);
            entity.HasIndex(d => d.UploadedAt);
        });

        // Configure ProjectComment entity
        modelBuilder.Entity<ProjectComment>(entity =>
        {
            entity.ToTable("DecVCPlat_ProjectComments");
            entity.HasKey(c => c.Id);
            
            entity.Property(c => c.Content).IsRequired().HasMaxLength(2000);
            entity.Property(c => c.CommentType).HasConversion<string>();
            
            entity.HasOne(c => c.Project)
                  .WithMany(p => p.Comments)
                  .HasForeignKey(c => c.ProjectId)
                  .OnDelete(DeleteBehavior.Cascade);
            
            // Self-referencing relationship for replies
            entity.HasOne(c => c.ParentComment)
                  .WithMany(c => c.Replies)
                  .HasForeignKey(c => c.ParentCommentId)
                  .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasIndex(c => c.ProjectId);
            entity.HasIndex(c => c.AuthorId);
            entity.HasIndex(c => c.CreatedAt);
            entity.HasIndex(c => c.ParentCommentId);
        });

        // Configure ProjectFunding entity
        modelBuilder.Entity<ProjectFunding>(entity =>
        {
            entity.ToTable("DecVCPlat_ProjectFundings");
            entity.HasKey(f => f.Id);
            
            entity.Property(f => f.Amount).HasPrecision(18, 2);
            entity.Property(f => f.StakedTokens).HasPrecision(18, 8);
            entity.Property(f => f.VotingPowerGranted).HasPrecision(18, 8);
            entity.Property(f => f.FundingType).HasConversion<string>();
            entity.Property(f => f.Status).HasConversion<string>();
            
            entity.HasOne(f => f.Project)
                  .WithMany(p => p.FundingRounds)
                  .HasForeignKey(f => f.ProjectId)
                  .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(f => f.Milestone)
                  .WithMany()
                  .HasForeignKey(f => f.MilestoneId)
                  .OnDelete(DeleteBehavior.SetNull);
            
            entity.HasIndex(f => f.ProjectId);
            entity.HasIndex(f => f.InvestorId);
            entity.HasIndex(f => f.Status);
            entity.HasIndex(f => f.FundedAt);
        });

        // Configure precision for all decimal properties
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var decimalProperties = entityType.GetProperties()
                .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?));
            
            foreach (var property in decimalProperties)
            {
                if (property.GetPrecision() == null)
                {
                    property.SetPrecision(18);
                    property.SetScale(2);
                }
            }
        }

        // Seed initial data for DecVCPlat categories
        SeedProjectCategories(modelBuilder);
    }

    private static void SeedProjectCategories(ModelBuilder modelBuilder)
    {
        // This would typically be in a separate Categories table, but for simplicity
        // we're using a string field. In production, consider normalizing this.
        
        // Common categories for venture capital projects
        var categories = new[]
        {
            "Fintech", "Healthtech", "EdTech", "CleanTech", "AI/ML",
            "Blockchain", "IoT", "SaaS", "E-commerce", "Gaming",
            "Cybersecurity", "Biotech", "PropTech", "AgriTech", "Other"
        };
        
        // Categories would be used for validation in the application layer
    }
}
