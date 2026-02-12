using JobBank.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobBank.ModelConfiguration
{
    public class UserSkillMatchReportConfiguration : IEntityTypeConfiguration<UserSkillMatchReport>
    {
        public void Configure(EntityTypeBuilder<UserSkillMatchReport> builder)
        {
            builder
                .Property(b => b.Version)
                .HasDefaultValue(1);

            builder
                .Property(b => b.CreatedDate)
                .HasDefaultValueSql("GETUTCDATE()");

             builder
                .HasIndex(b => b.Hash)               
                .IsUnique();
        }
    }
}
