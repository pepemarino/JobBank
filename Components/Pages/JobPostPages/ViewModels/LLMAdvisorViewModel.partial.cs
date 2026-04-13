namespace JobBank.Components.Pages.JobPostPages.ViewModels
{
    public partial class LLMAdvisorViewModel
    {
        public enum TearModel
        {
            Legacy,
            Free,
            Paid
        }   

        private const string interviewPreparationQuestions = "You are a technical recruiter. Analyze the JobDescription. " +
    "1. Generate 5 behavioral/technical questions. " +
    "2. List 5 study subjects. " +
    "3. Generate 2 questions to ask employer. " +
    "Return ONLY a valid JSON object with keys InterviewQuestions, StudySubjects, and EmployerQuestions. No markdown.";
    }
}
