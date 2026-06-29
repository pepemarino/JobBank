using JobBank.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobBank.ModelConfiguration
{
    public class InterviewConfiguration : IEntityTypeConfiguration<Interview>
    {
        public void Configure(EntityTypeBuilder<Interview> builder)
        {
            builder.Property(b => b.UserId)
                .IsRequired();

            builder.Property(b => b.Result)
                .IsRequired();

            builder.Property(b => b.Model)
                .IsRequired();

            builder.Property(b => b.Prompt)
                .IsRequired();

            builder.Property(b => b.IsDeleted)
                .HasDefaultValue(false);

            builder.Property(b => b.IsMigrated)
                .HasDefaultValue(false);

            builder.HasIndex(x => x.IsMigrated);

            builder
                .HasMany(b => b.Evaluations)
                .WithOne(i => i.Interview)
                .HasForeignKey(i => i.InterviewId)
                .OnDelete(DeleteBehavior.Cascade);

            builder
                .Property(b => b.CreatedDateUtc)
                .HasDefaultValueSql("GETUTCDATE()")
                .ValueGeneratedOnAdd();
        }
    }
}
