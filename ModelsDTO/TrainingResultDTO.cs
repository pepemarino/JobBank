namespace JobBank.ModelsDTO
{
    public class InterviewTrainingAnalysisResultDTO
    {
        public List<TrainingResultDTO> Training { get; set; } = new();
        public string ErrorMessage { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
    }

    public class TrainingResultDTO
    {
        public string TrainingTopic { get; set; } = string.Empty;
        public List<Prerequisite> Prerequisites { get; set; } = new();
        public string TrainingSource { get; set; } = string.Empty;

        public string TrainingType { get; set; } = string.Empty;

        public string Abstract { get; set; } = string.Empty;

        public List<string> WhereToFocus { get; set; } = new();

        public List<string> HomeworkQuestions { get; set; } = new ();

        public required MasteryTask MasteryTask { get; set; }
    }

    public class MasteryTask
    {
        public string Topic { get; set; } = string.Empty;
        public List<string> EssentialSubtopics { get; set; } = new();
    }

    public class Prerequisite
    {
        public string ReferenceTitle { get; set; } = string.Empty;
        public string ReferenceSource { get; set; } = string.Empty;
        public string ReferenceType { get; set; } = string.Empty;
        public string ReferenceLink { get; set; } = string.Empty;
    }
}
