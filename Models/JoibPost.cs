using JobBank.ModelConfiguration;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace JobBank.Models
{
    [EntityTypeConfiguration(typeof(JobPostConfiguration))]
    public class JobPost
    {
        public int Id { get; set; }

        [Required]
        public string? Title { get; set; }
        public bool IsApplied { get; set; }
        public string? Description { get; set; }

        [Required]
        public string? Company { get; set; }
        public string? Impression { get; set; }
        public string? ActionToTake { get; set; }

        [Required]
        public string? JobType { get; set; }
        public string? JobBase { get; set; }
        public string? ApplicationType { get; set; }

        [Required]
        public DateTime? ApplicationDate { get; set; }
        public DateTime? ResponseDate { get; set; }
        public DateTime? InterviewDate { get; set; }
        public DateTime? Timestamp { get; set; }
        public string? InterviewType { get; set; }
        public string? InterviewOutcome { get; set; }
        public bool ItHasChallenge { get; set; }
        public string? ContactPhone { get; set; }
        public string? ContactEmail { get; set; }
        public string? Comments { get; set; }

        public bool ApplicationDeclined { get; set; }

        public virtual UserSkillMatchReport? UserSkillMatchReport { get; set; }
        public virtual JobRejectionAnalysis? JobRejectionAnalysis { get; set; }
    }
}
