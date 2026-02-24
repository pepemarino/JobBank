namespace JobBank.ModelsDTO
{
    public class JobRejectionAnalysisDTO
    {
        public int Id { get; set; }

        public string ApplicantSkills { get; set; } = string.Empty;
        public string Analisis { get; set; } = string.Empty;
        public string JobDescription { get; set; } = string.Empty;
        public bool IsProcessed { get; set; } = false;

        public int Version { get; set; } = 1;
        public string ModelUsed { get; set; } = string.Empty;
        public string PromptVersion { get; set; } = string.Empty;

        public int JobPostId { get; set; }
    }
}
