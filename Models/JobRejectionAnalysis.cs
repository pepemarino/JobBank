using JobBank.ModelConfiguration;
using Microsoft.EntityFrameworkCore;

namespace JobBank.Models
{
    [EntityTypeConfiguration(typeof(JobRejectionAnalysisConfiguration))]
    public class JobRejectionAnalysis
    {
        public int Id { get; set; }

        /// <summary>
        /// These are the skills of the applicant at the time of the analysis
        /// </summary>
        public string ApplicantSkills { get; set; } = string.Empty;
        public string Analisis { get; set; } = string.Empty;
        public string JobDescription { get; set; } = string.Empty;
        public bool IsProcessed { get; set; } = false;
        public DateTime AnalysisDate { get; set; }

        public int Version { get; set; } = 1;
        public string ModelUsed { get; set; } = string.Empty;
        public string PromptVersion { get; set; } = string.Empty;

        public int JobPostId { get; set; }
    }
}
