namespace JobBank.ModelsDTO
{
    public class JobAnalysisCacheDTO
    {
        public string? Hash { get; set; }
        public string? UserId { get; set; }
        public string? JobPostDescription { get; set; }
        public string? ModelUsed { get; set; }
        public string? PromptVersion { get; set; }
        public string? Result { get; set; }
        public bool IsLegacy { get; set; } 
        public bool IsPublic { get; set; } 
        public bool IsDonated { get; set; }
        public bool IsValid { get; set; } 
        public string? State { get; set; } 
        public string? Richness { get; set; }
        public string SourceModelTier { get; set; } = string.Empty; 
    }
}
