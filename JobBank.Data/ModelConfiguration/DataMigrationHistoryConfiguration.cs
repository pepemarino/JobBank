using JobBank.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobBank.Data.ModelConfiguration
{
    internal class DataMigrationHistoryConfiguration : IEntityTypeConfiguration<DataMigrationHistory>
    {
        public void Configure(EntityTypeBuilder<DataMigrationHistory> builder)
        {
            builder
                .Property(b => b.MigrationName)
                .IsRequired();

            builder.HasIndex(x => x.MigrationName)
                .IsUnique();

            builder
                .Property(b => b.AppliedAt)
                .HasDefaultValueSql("GETUTCDATE()")
                .ValueGeneratedOnAdd();
        }
    }
}
