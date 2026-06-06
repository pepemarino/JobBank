using JobBank.Models.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobBank.ModelConfiguration
{
    public class JobBankUserConfiguration : IEntityTypeConfiguration<JobBankUser>
    {
        public void Configure(EntityTypeBuilder<JobBankUser> builder)
        {
            builder
               .Property(b => b.UserCreated)
               .HasDefaultValueSql("GETUTCDATE()");
            builder
               .Property(b => b.UserUpdated)
               .HasDefaultValueSql("GETUTCDATE()");
        }
    }
}
