using JobBank.Components.Pages.Init;
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

        string JobDescription { get; set; }

        string InterviewAgentQuestion { get; set; }
        string? InterviewAnswer { get; set; }

        bool ShouldPreventDefault { get; set; }

        bool IsProcessing { get; set; }
        string? ResponseMessage { get; set; }

        Task ProcessAnswerAsync(MouseEventArgs args);

        Task SendAnswerAsync(KeyboardEventArgs e);
    }
}
