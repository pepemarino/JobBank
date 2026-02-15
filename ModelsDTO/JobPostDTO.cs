using JobBank.Models;

namespace JobBank.ModelsDTO
{
    public class JobPostDTO
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public bool IsApplied { get; set; }
        public string? Description { get; set; }
        public string? Company { get; set; }
        public string? Impression { get; set; }
        public string? ActionToTake { get; set; }
        public string? JobType { get; set; }
        public string? JobBase { get; set; }
        public string? ApplicationType { get; set; }
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

        public UserSkillMatchReportDTO? UserSkillMatchReport { get; set; }
    }
}
