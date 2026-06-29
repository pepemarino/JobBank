namespace JobBank.Management.Abstraction
{
    public interface ITrainerAssistantManager
    {
        Task<int> AnalyzeInterviewAsync(string userId, int interviewId, string? prompt = null);
    }
}
