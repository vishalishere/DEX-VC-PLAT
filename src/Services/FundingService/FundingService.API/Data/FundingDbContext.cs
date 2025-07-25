// © 2024 DecVCPlat. All rights reserved.

using Microsoft.EntityFrameworkCore;
using FundingService.API.Models.Entities;

namespace FundingService.API.Data;

public class FundingDbContext : DbContext
{
    public FundingDbContext(DbContextOptions<FundingDbContext> options) : base(options)
    {
    }

    public DbSet<FundingTranche> FundingTranches { get; set; }
    public DbSet<FundingRelease> FundingReleases { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure FundingTranche
        modelBuilder.Entity<FundingTranche>(entity =>
        {
            entity.ToTable("DecVCPlat_FundingTranches");
            entity.HasKey(t => t.Id);
            
            entity.Property(t => t.Title).IsRequired().HasMaxLength(200);
            entity.Property(t => t.Description).HasMaxLength(1000);
            entity.Property(t => t.Amount).HasPrecision(18, 2);
            entity.Property(t => t.Status).HasConversion<string>();
            entity.Property(t => t.MinVotingThreshold).HasPrecision(5, 2);
            entity.Property(t => t.ProcessingFee).HasPrecision(18, 2);
            entity.Property(t => t.ActualAmountReleased).HasPrecision(18, 2);
            
            entity.HasIndex(t => t.ProjectId);
            entity.HasIndex(t => t.MilestoneId);
            entity.HasIndex(t => t.Status);
            entity.HasIndex(t => t.ScheduledReleaseDate);
            entity.HasIndex(t => t.TrancheNumber);
        });

        // Configure FundingRelease
        modelBuilder.Entity<FundingRelease>(entity =>
        {
            entity.ToTable("DecVCPlat_FundingReleases");
            entity.HasKey(r => r.Id);
            
            entity.Property(r => r.Amount).HasPrecision(18, 2);
            entity.Property(r => r.Status).HasConversion<string>();
            entity.Property(r => r.PaymentMethod).HasConversion<string>();
            entity.Property(r => r.GasFee).HasPrecision(18, 8);
            entity.Property(r => r.ProcessingFeeCharged).HasPrecision(18, 2);
            
            entity.HasOne(r => r.Tranche)
                  .WithMany(t => t.Releases)
                  .HasForeignKey(r => r.TrancheId)
                  .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasIndex(r => r.TrancheId);
            entity.HasIndex(r => r.ProjectId);
            entity.HasIndex(r => r.Status);
            entity.HasIndex(r => r.InitiatedAt);
            entity.HasIndex(r => r.TransactionHash);
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
    }
}
