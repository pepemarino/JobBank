using JobBank.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobBank.ModelConfiguration
{
    public class TrainingConfiguration : IEntityTypeConfiguration<Training>
    {
        public void Configure(EntityTypeBuilder<Training> builder)
        {
            builder.HasKey(b => b.Id);

            builder.Property(b => b.UserId)
            .IsRequired();

            builder.Property(b => b.Result)
                .IsRequired();

            builder
                .Property(b => b.CreatedDateUtc)
                .HasDefaultValueSql("GETUTCDATE()")
                .ValueGeneratedOnAdd();

            builder.Property(b => b.Model)
                .IsRequired();

            builder.Property(b => b.Prompt)
                .IsRequired();

            builder.Property(b => b.IsDeleted)
                .HasDefaultValue(false);

            builder.HasOne(b => b.Interview)
                .WithOne(i => i.Training)
                .HasForeignKey<Training>(b => b.InterviewId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
