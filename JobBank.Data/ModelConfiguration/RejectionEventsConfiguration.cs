using JobBank.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobBank.Data.ModelConfiguration
{
    public class RejectionEventsConfiguration : IEntityTypeConfiguration<RejectionEvents>
    {
        public void Configure(EntityTypeBuilder<RejectionEvents> builder)
        {
            builder.HasKey(e => e.Id);

            builder.Property(e => e.JobId)
                .IsRequired();

            builder.Property(e => e.UserId)
                .IsRequired()
                .HasMaxLength(128);

            builder.Property(e => e.IsProcessed)
                .HasDefaultValue(false);

            builder.Property(e => e.TerminationReason)
                .IsRequired()
                .HasMaxLength(256);

            builder.Property(e => e.EventDate)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(e => e.Timestamp)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");
        }
    }
}