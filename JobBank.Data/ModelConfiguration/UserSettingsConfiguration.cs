using JobBank.Models.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobBank.ModelConfiguration
{
    public class UserSettingsConfiguration : IEntityTypeConfiguration<UserSettings>
    {
        public void Configure(EntityTypeBuilder<UserSettings> builder)
        {
            builder
               .Property(b => b.CreatedDateTime)
               .HasDefaultValueSql("GETUTCDATE()");
            builder
               .Property(b => b.UpdatedDateTime)
               .HasDefaultValueSql("GETUTCDATE()");
        }
    }
}
