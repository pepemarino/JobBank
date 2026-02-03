using Microsoft.EntityFrameworkCore;

namespace JobBank.Models
{
    [Index(nameof(Hash), IsUnique = true)]
    public class JobAnalysisCache
    {
        public int Id { get; set; }
        public string? Hash { get; set; }
        public string? JobPostDescription { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? ModelUsed { get; set; }
        public string? PromptVersion { get; set; }
        public string? Result { get; set; }
    }
}
