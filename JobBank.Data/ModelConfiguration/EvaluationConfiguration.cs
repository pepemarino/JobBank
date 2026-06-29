using JobBank.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobBank.ModelConfiguration
{
    public class EvaluationConfiguration : IEntityTypeConfiguration<Evaluation>
    {
        public void Configure(EntityTypeBuilder<Evaluation> builder)
        {
            builder.Property(b => b.PreviousQuestion)
                .IsRequired();

            builder.Property(b => b.PreviousTopic)
                .IsRequired();

            builder.Property(b => b.Strengths)
                .IsRequired();

            builder.Property(b => b.Gaps)
                .IsRequired();

            builder.Property(b => b.Evidence)
                .IsRequired();

            builder.Property(b => b.Confidence)
                .HasDefaultValue(0.0);

            builder.Property(b => b.Score)
                .HasDefaultValue(0.0);

            builder
                .Property(b => b.CreatedDateUtc)
                .HasDefaultValueSql("GETUTCDATE()")
                .ValueGeneratedOnAdd();
        }
    }
}
