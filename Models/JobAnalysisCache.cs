using Microsoft.AspNetCore.Routing.Matching;
using Microsoft.EntityFrameworkCore;

namespace JobBank.Models
{
    [Index(nameof(Hash), IsUnique = false)]
    public class JobAnalysisCache
    {
        public int Id { get; set; }
        public string? UserId { get; set; }
        public string? Hash { get; set; }
        public string? JobPostDescription { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? ModelUsed { get; set; }
        public string? PromptVersion { get; set; }
        public string? Result { get; set; }
        public bool IsLegacy { get; set; } // Indicates if this cache entry was created before authentication was implemented
        public bool IsPublic { get; set; } // Indicates if this cache entry is public (available to all users) or private (tied to a specific user)
        public bool IsDonated { get; set; } // Indicates if this cache entry was donated by a user for public use
        public bool IsValid { get; set; } // This is in relation to donation. Is this a vslid best cache than systen provided?
        public string? State { get; set; } // This is in relation to donation.  Is this pending review, approved, rejected?
        public string? Richness { get; set; } // This is in relation to donation.  Does this have just the basic cache info or does it also have additional information that would make it more useful to other users?
        public string SourceModelTier { get; set; } = string.Empty; // Indicates the source Legacy-system
    }
}
