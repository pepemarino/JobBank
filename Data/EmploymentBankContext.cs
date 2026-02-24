using JobBank.Models;
using Microsoft.EntityFrameworkCore;

namespace JobBank.Data
{
    public class EmploymentBankContext : DbContext
    {
        public EmploymentBankContext (DbContextOptions<EmploymentBankContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(EmploymentBankContext).Assembly);
        }

        public DbSet<JobPost> JobPost { get; set; } = default!;

        public DbSet<JobAnalysisCache> JobAnalysisCache { get; set; } = default!;

        public DbSet<UserSkillMatchReport> UserSkillMatchReport { get; set;} = default!;

        public DbSet<UserSkills> UserSkills { get; set; } = default!;

        public DbSet<JobRejectionAnalysis> JobRejectionAnalyses { get; set; } = default!;
    }
}
