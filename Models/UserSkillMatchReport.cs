using JobBank.ModelConfiguration;
using Microsoft.EntityFrameworkCore;

namespace JobBank.Models
{
    [EntityTypeConfiguration(typeof(UserSkillMatchReportConfiguration))]
    public class UserSkillMatchReport
    {
        public int Id { get; set; }
        public string Hash { get; set; }
        public string RawSkillSet { get; set; } = string.Empty;
        public int Version { get; set; } = 1;
        public DateTime CreatedDate { get; set; }
        public string? ModelUsed { get; set; }
        public string? PromptVersion { get; set; }
        public string? Result { get; set; }
    }
}
