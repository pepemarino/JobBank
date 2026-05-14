using JobBank.Components.Pages.Init;
using JobBank.Management.Interview;
using JobBank.ModelsDTO;
using Microsoft.AspNetCore.Components.QuickGrid;

namespace JobBank.Components.Pages.InterviewLibrary.ViewModels
{
    public interface IInterviewLibraryViewModel : IAsyncInitialization
    {
        ValueTask<GridItemsProviderResult<InterviewDTO>> GetInterviews(GridItemsProviderRequest<InterviewDTO> request);
        string CompanySearch { get; set; }
        PaginationState Pagination { get; }
        string? GetRowCssClass(InterviewDTO jobPost);

        List<ChatMessage> History { get; set; }

        InterviewTrainingAnalysisResultDTO TrainingAnalysys { get; set; }

        Task SelectInterviewAsync(InterviewDTO args);

        bool IsInterview { get; set; }

        bool IsTraining { get; set; }

        Task LoadTraining(InterviewDTO interview);

        public event Action? OnRequestUIUpdate;
    }
}
