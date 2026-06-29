namespace JobBank.ModelsDTO
{
    public class UserSkillMatchReportDTO
    {
        public int Id { get; set; }

        /// <summary>
        /// This hash is generated from Denormalized UserSkills
        /// </summary>
        public string Hash { get; set; }
        public string RawSkillSet { get; set; } = string.Empty;

        public string JobDescription { get; set; } = string.Empty;

        public int Version { get; set; } = 1;
        public string? ModelUsed { get; set; }
        public string? PromptVersion { get; set; }
        public string? Result { get; set; }

        public int JobPostId { get; set; }
    }
}
