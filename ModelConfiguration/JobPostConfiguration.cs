using JobBank.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobBank.ModelConfiguration
{
    public class JobPostConfiguration : IEntityTypeConfiguration<JobPost>
    {
        public void Configure(EntityTypeBuilder<JobPost> builder)
        {
            builder
                .Property(b => b.Title)
                .IsRequired();
            builder
                .Property(b => b.Company)
                .IsRequired();
            builder
                .Property(b => b.JobType)
                .IsRequired();
             builder
                .Property(b => b.ApplicationDate)
                .IsRequired();
            builder.Property(b => b.UserId)
                .IsRequired();
            builder
                .Property(b => b.Timestamp)
                .HasDefaultValueSql("GETUTCDATE()");
            builder
                .HasMany(b => b.Interviews)
                .WithOne(i => i.JobPost)
                .HasForeignKey(i => i.JobPostId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
