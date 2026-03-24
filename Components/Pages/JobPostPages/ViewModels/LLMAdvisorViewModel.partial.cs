namespace JobBank.Components.Pages.JobPostPages.ViewModels
{
    public partial class LLMAdvisorViewModel
    {
        private const string interviewPreparationQuestions = "You are a technical recruiter. Analyze the JD. " +
    "1. Generate 5 behavioral/technical questions. " +
    "2. List 5 study subjects. " +
    "Return ONLY a valid JSON object with keys InterviewQuestions and StudySubjects. No markdown.";
    }
}
