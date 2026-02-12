using JobBank.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobBank.ModelConfiguration
{
    public class UserSkillsConfiguration : IEntityTypeConfiguration<UserSkills>
    {
        public void Configure(EntityTypeBuilder<UserSkills> builder)
        {
            builder
                .Property(b => b.Version)
                .HasDefaultValue(1);

            builder
                .Property(b => b.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()")
                .ValueGeneratedOnAdd();
        }
    }
}