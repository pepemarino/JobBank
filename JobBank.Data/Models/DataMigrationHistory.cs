using JobBank.Data.ModelConfiguration;
using Microsoft.EntityFrameworkCore;

namespace JobBank.Models
{
    [EntityTypeConfiguration(typeof(DataMigrationHistoryConfiguration))]
    public class DataMigrationHistory
    {
        public int Id { get; set; }
        public string MigrationName { get; set; } = string.Empty;
        public DateTime AppliedAt { get; set; } = DateTime.UtcNow;
        public string? Remarks { get; set; }
    }
}
