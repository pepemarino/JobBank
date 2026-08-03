using JobBank.Data.ModelConfiguration;
using Microsoft.EntityFrameworkCore;

namespace JobBank.Models
{
    [EntityTypeConfiguration(typeof(RejectionEventsConfiguration))]
    public class RejectionEvents
    {
        public int Id { get; set; }
        public int JobId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string TerminationReason { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public DateTime Timestamp { get; set; }
        public bool IsProcessed { get; set; }
    }
}
