using JobBank.Management.Interview;

namespace JobBank.ModelsDTO
{
    public class TrainerAnalysisMetadataDTO
    {
        public Dictionary<string, string> JobDescriptionDictionarySkills { get; set; } = new();
        public List<string> CanonicalApplicantSkills { get; set; } = new();

        #region Interview State Tracking
        public List<string> CoveredTopics { get; set; } = new();
        public List<string> WeakAreas { get; set; } = new();
        public List<EvaluationResult> Evaluations { get; set; } = new();

        #endregion Interview State Tracking
    }
}
