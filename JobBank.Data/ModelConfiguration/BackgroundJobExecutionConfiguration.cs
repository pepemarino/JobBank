using JobBank.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobBank.ModelConfiguration
{
    public class BackgroundJobExecutionConfiguration : IEntityTypeConfiguration<BackgroundJobExecution>
    {
        public void Configure(EntityTypeBuilder<BackgroundJobExecution> builder)
        {
            builder.HasKey(e => e.Id);

            builder.Property(e => e.JobName)
                .IsRequired()
                .HasMaxLength(256);

            builder.Property(e => e.LastExecutionDate)
                .IsRequired();

            builder.Property(e => e.CreatedDateTime)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(e => e.UpdatedDateTime)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");
        }
    }
}