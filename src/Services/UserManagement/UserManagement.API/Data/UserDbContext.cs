// © 2024 DecVCPlat. All rights reserved.

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Shared.Security.Models;

namespace UserManagement.API.Data;

public class UserDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
{
    public UserDbContext(DbContextOptions<UserDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Custom table names for DecVCPlat
        builder.Entity<ApplicationUser>().ToTable("DecVCPlat_Users");
        builder.Entity<ApplicationRole>().ToTable("DecVCPlat_Roles");
        builder.Entity<IdentityUserRole<Guid>>().ToTable("DecVCPlat_UserRoles");
        builder.Entity<IdentityUserClaim<Guid>>().ToTable("DecVCPlat_UserClaims");
        builder.Entity<IdentityUserLogin<Guid>>().ToTable("DecVCPlat_UserLogins");
        builder.Entity<IdentityRoleClaim<Guid>>().ToTable("DecVCPlat_RoleClaims");
        builder.Entity<IdentityUserToken<Guid>>().ToTable("DecVCPlat_UserTokens");

        // Configure ApplicationUser properties
        builder.Entity<ApplicationUser>(entity =>
        {
            entity.HasIndex(u => u.Email).IsUnique();
            entity.HasIndex(u => u.WalletAddress).IsUnique().HasFilter("WalletAddress IS NOT NULL");
            
            entity.Property(u => u.Role).HasConversion<string>();
            entity.Property(u => u.Status).HasConversion<string>();
            entity.Property(u => u.MinInvestmentAmount).HasPrecision(18, 2);
            entity.Property(u => u.MaxInvestmentAmount).HasPrecision(18, 2);
        });

        // Seed DecVCPlat roles
        var founderRoleId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var investorRoleId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var luminaryRoleId = Guid.Parse("33333333-3333-3333-3333-333333333333");

        builder.Entity<ApplicationRole>().HasData(
            new ApplicationRole
            {
                Id = founderRoleId,
                Name = "Founder",
                NormalizedName = "FOUNDER",
                Description = "DecVCPlat Founders - Submit projects and request funding",
                CreatedAt = DateTime.UtcNow
            },
            new ApplicationRole
            {
                Id = investorRoleId,
                Name = "Investor",
                NormalizedName = "INVESTOR",
                Description = "DecVCPlat Investors - Stake tokens and vote on projects",
                CreatedAt = DateTime.UtcNow
            },
            new ApplicationRole
            {
                Id = luminaryRoleId,
                Name = "Luminary",
                NormalizedName = "LUMINARY",
                Description = "DecVCPlat Luminaries - Review and approve project funding",
                CreatedAt = DateTime.UtcNow
            }
        );
    }
}
