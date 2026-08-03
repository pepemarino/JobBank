using JobBank.ModelConfiguration;
using Microsoft.EntityFrameworkCore;

namespace JobBank.Models
{
    [EntityTypeConfiguration(typeof(BackgroundJobExecutionConfiguration))]
    public class BackgroundJobExecution
    {
        public int Id { get; set; }

        /// <summary>
        /// Unique identifier for the background job (e.g., "RejectionWorker", "TrainerAnalysisWorker")
        /// </summary>
        public string JobName { get; set; } = string.Empty;

        /// <summary>
        /// The last time this job executed successfully
        /// </summary>
        public DateTime LastExecutionDate { get; set; }

        /// <summary>
        /// When this record was created
        /// </summary>
        public DateTime CreatedDateTime { get; set; }

        /// <summary>
        /// When this record was last updated
        /// </summary>
        public DateTime UpdatedDateTime { get; set; }
    }
}