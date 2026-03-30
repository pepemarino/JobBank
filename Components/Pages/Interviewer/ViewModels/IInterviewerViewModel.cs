using JobBank.Components.Pages.Init;
using JobBank.Management.Interview;
using Microsoft.AspNetCore.Components.Web;

namespace JobBank.Components.Pages.Interviewer.ViewModels
{
    public interface IInterviewerViewModel : IAsyncInitialization
    {
        public record ChatMessage(string Role, string Content, DateTime Timestamp);

        List<ChatMessage> History { get; set; }

        string Title { get; set; }
        int JobPostId { get; set; }

        string CompanyName { get; set; }

        string JobTitle { get; set; }

        int QuestionCount { get; set; }

        int QuestionMax { get; }

        bool IsInterviewCompleted { get; }

        string QuestionProgressCounter { get; }

        string JobDescription { get; set; }

        string InterviewAgentQuestion { get; set; }
        string? InterviewAnswer { get; set; }
        string QuestionTopic { get; set; }

        bool IsJobDescriptionAvailable { get; set; }

        bool IsProcessing { get; set; }
        string? ResponseMessage { get; set; }

        Task ProcessAnswerAsync(MouseEventArgs args);

    }
}
