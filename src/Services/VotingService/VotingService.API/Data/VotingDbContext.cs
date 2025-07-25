// © 2024 DecVCPlat. All rights reserved.

using Microsoft.EntityFrameworkCore;
using VotingService.API.Models.Entities;

namespace VotingService.API.Data;

public class VotingDbContext : DbContext
{
    public VotingDbContext(DbContextOptions<VotingDbContext> options) : base(options)
    {
    }

    public DbSet<VotingProposal> VotingProposals { get; set; }
    public DbSet<TokenStake> TokenStakes { get; set; }
    public DbSet<Vote> Votes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure VotingProposal
        modelBuilder.Entity<VotingProposal>(entity =>
        {
            entity.ToTable("DecVCPlat_VotingProposals");
            entity.HasKey(p => p.Id);
            
            entity.Property(p => p.Title).IsRequired().HasMaxLength(200);
            entity.Property(p => p.Description).HasMaxLength(2000);
            entity.Property(p => p.ProposalType).HasConversion<string>();
            entity.Property(p => p.Status).HasConversion<string>();
            
            entity.Property(p => p.MinTokensRequired).HasPrecision(18, 8);
            entity.Property(p => p.QuorumRequired).HasPrecision(5, 2);
            entity.Property(p => p.TotalTokensStaked).HasPrecision(18, 8);
            entity.Property(p => p.SupportTokens).HasPrecision(18, 8);
            entity.Property(p => p.AgainstTokens).HasPrecision(18, 8);
            entity.Property(p => p.AbstainTokens).HasPrecision(18, 8);
            
            entity.HasIndex(p => p.ProjectId);
            entity.HasIndex(p => p.Status);
            entity.HasIndex(p => p.StartDate);
            entity.HasIndex(p => p.EndDate);
        });

        // Configure TokenStake
        modelBuilder.Entity<TokenStake>(entity =>
        {
            entity.ToTable("DecVCPlat_TokenStakes");
            entity.HasKey(s => s.Id);
            
            entity.Property(s => s.UserName).HasMaxLength(100);
            entity.Property(s => s.WalletAddress).IsRequired().HasMaxLength(200);
            entity.Property(s => s.TokenAmount).HasPrecision(18, 8);
            entity.Property(s => s.Status).HasConversion<string>();
            entity.Property(s => s.RewardEarned).HasPrecision(18, 8);
            entity.Property(s => s.PenaltyApplied).HasPrecision(18, 8);
            
            entity.HasOne(s => s.Proposal)
                  .WithMany(p => p.TokenStakes)
                  .HasForeignKey(s => s.ProposalId)
                  .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasIndex(s => s.ProposalId);
            entity.HasIndex(s => s.UserId);
            entity.HasIndex(s => s.WalletAddress);
            entity.HasIndex(s => s.Status);
        });

        // Configure Vote
        modelBuilder.Entity<Vote>(entity =>
        {
            entity.ToTable("DecVCPlat_Votes");
            entity.HasKey(v => v.Id);
            
            entity.Property(v => v.VoterName).HasMaxLength(100);
            entity.Property(v => v.Choice).HasConversion<int>();
            entity.Property(v => v.VotingPower).HasPrecision(18, 8);
            entity.Property(v => v.BaseVotingPower).HasPrecision(18, 8);
            entity.Property(v => v.StakeMultiplier).HasPrecision(5, 2);
            entity.Property(v => v.ReputationMultiplier).HasPrecision(5, 2);
            
            entity.HasOne(v => v.Proposal)
                  .WithMany(p => p.Votes)
                  .HasForeignKey(v => v.ProposalId)
                  .OnDelete(DeleteBehavior.Cascade);
            
            // Unique constraint: one vote per voter per proposal
            entity.HasIndex(v => new { v.ProposalId, v.VoterId }).IsUnique();
            entity.HasIndex(v => v.CastAt);
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
                    property.SetScale(8);
                }
            }
        }
    }
}
