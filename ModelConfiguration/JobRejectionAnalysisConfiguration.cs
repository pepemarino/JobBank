using JobBank.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobBank.ModelConfiguration
{
    public class JobRejectionAnalysisConfiguration : IEntityTypeConfiguration<JobRejectionAnalysis>
    {
        public void Configure(EntityTypeBuilder<JobRejectionAnalysis> builder)
        {
            builder
                .Property(b => b.ApplicantSkills)
                .IsRequired();
            builder
                .Property(b => b.IsProcessed)
                .HasDefaultValue(false);
            builder
                .Property(b => b.AnalysisDate)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");
        }
    }
}
