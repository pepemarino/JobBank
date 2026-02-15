using JobBank.ModelConfiguration;
using Microsoft.EntityFrameworkCore;

namespace JobBank.Models
{
    [EntityTypeConfiguration(typeof(UserSkillsConfiguration))]
    public class UserSkills
    {
        public int Id { get; set; }
        public int? UserId { get; set; }

        /// <summary>
        /// Try to store raw text, this way pass to the LLM clean text 
        /// It will avoid the consumption of useless tokens
        /// </summary>
        public string RawSkills { get; set; } = string.Empty;

        public int Version { get; set; } = 1;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; } // 
    }
}
