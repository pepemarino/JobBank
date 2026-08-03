using JobBank.Models;
using JobBank.Models.Identity;
using Microsoft.EntityFrameworkCore;

namespace JobBank.Data
{
    public class EmploymentBankContext : DbContext
    {
        public EmploymentBankContext(DbContextOptions<EmploymentBankContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(EmploymentBankContext).Assembly);
        }

        public DbSet<JobPost> JobPost { get; set; } = default!;

        public DbSet<JobAnalysisCache> JobAnalysisCache { get; set; } = default!;

        public DbSet<UserSkillMatchReport> UserSkillMatchReport { get; set; } = default!;

        public DbSet<UserSkills> UserSkills { get; set; } = default!;

        public DbSet<JobRejectionAnalysis> JobRejectionAnalyses { get; set; } = default!;

        public DbSet<Interview> Interviews { get; set; } = default!;

        public DbSet<Training> Trainings { get; set; } = default!;

        public DbSet<UserSettings> UserSettings { get; set; } = default!;

        public DbSet<Evaluation> Evaluations { get; set; } = default!;

        public DbSet<DataMigrationHistory> DataMigrationHistories { get; set; } = default!;

        public DbSet<BackgroundJobExecution> BackgroundJobExecutions { get; set; } = default!;

        public DbSet<RejectionEvents> RejectionEvents { get; set; } = default!;
    }
}
