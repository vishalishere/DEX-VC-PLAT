using Microsoft.EntityFrameworkCore;

namespace DecVCPlat.Common.Data
{
    /// <summary>
    /// Provides extension methods for configuring database indexes to optimize query performance
    /// </summary>
    public static class DatabaseIndexConfiguration
    {
        /// <summary>
        /// Configures common indexes for the User Management database
        /// </summary>
        /// <param name="modelBuilder">The model builder</param>
        public static void ConfigureUserManagementIndexes(this ModelBuilder modelBuilder)
        {
            // User indexes
            modelBuilder.Entity("DecVCPlat.UserManagement.Models.User")
                .HasIndex(u => u.Email)
                .IsUnique();
                
            modelBuilder.Entity("DecVCPlat.UserManagement.Models.User")
                .HasIndex(u => u.Username)
                .IsUnique();
                
            // Role indexes
            modelBuilder.Entity("DecVCPlat.UserManagement.Models.UserRole")
                .HasIndex(r => new { r.UserId, r.RoleId })
                .IsUnique();
        }
        
        /// <summary>
        /// Configures common indexes for the Project Management database
        /// </summary>
        /// <param name="modelBuilder">The model builder</param>
        public static void ConfigureProjectManagementIndexes(this ModelBuilder modelBuilder)
        {
            // Project indexes
            modelBuilder.Entity("DecVCPlat.ProjectManagement.Models.Project")
                .HasIndex(p => p.Title);
                
            modelBuilder.Entity("DecVCPlat.ProjectManagement.Models.Project")
                .HasIndex(p => p.Status);
                
            // Project member indexes
            modelBuilder.Entity("DecVCPlat.ProjectManagement.Models.ProjectMember")
                .HasIndex(pm => new { pm.ProjectId, pm.UserId })
                .IsUnique();
        }
        
        /// <summary>
        /// Configures common indexes for the Voting database
        /// </summary>
        /// <param name="modelBuilder">The model builder</param>
        public static void ConfigureVotingIndexes(this ModelBuilder modelBuilder)
        {
            // Voting session indexes
            modelBuilder.Entity("DecVCPlat.Voting.Models.VotingSession")
                .HasIndex(vs => vs.ProjectId);
                
            modelBuilder.Entity("DecVCPlat.Voting.Models.VotingSession")
                .HasIndex(vs => vs.Status);
                
            // Vote indexes
            modelBuilder.Entity("DecVCPlat.Voting.Models.Vote")
                .HasIndex(v => new { v.VotingSessionId, v.UserId })
                .IsUnique();
        }
        
        /// <summary>
        /// Configures common indexes for the Funding database
        /// </summary>
        /// <param name="modelBuilder">The model builder</param>
        public static void ConfigureFundingIndexes(this ModelBuilder modelBuilder)
        {
            // Funding transaction indexes
            modelBuilder.Entity("DecVCPlat.Funding.Models.FundingTransaction")
                .HasIndex(ft => ft.ProjectId);
                
            modelBuilder.Entity("DecVCPlat.Funding.Models.FundingTransaction")
                .HasIndex(ft => ft.UserId);
                
            modelBuilder.Entity("DecVCPlat.Funding.Models.FundingTransaction")
                .HasIndex(ft => ft.TransactionHash);
        }
        
        /// <summary>
        /// Configures common indexes for the Notification database
        /// </summary>
        /// <param name="modelBuilder">The model builder</param>
        public static void ConfigureNotificationIndexes(this ModelBuilder modelBuilder)
        {
            // Notification indexes
            modelBuilder.Entity("DecVCPlat.Notification.Models.Notification")
                .HasIndex(n => n.UserId);
                
            modelBuilder.Entity("DecVCPlat.Notification.Models.Notification")
                .HasIndex(n => n.IsRead);
                
            // Notification subscription indexes
            modelBuilder.Entity("DecVCPlat.Notification.Models.NotificationSubscription")
                .HasIndex(ns => new { ns.UserId, ns.NotificationType })
                .IsUnique();
        }
    }
}
