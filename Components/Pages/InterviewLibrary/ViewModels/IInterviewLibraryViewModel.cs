using JobBank.Components.Pages.Init;
using JobBank.Management.Interview;
using JobBank.ModelsDTO;
using Microsoft.AspNetCore.Components.QuickGrid;

namespace JobBank.Components.Pages.InterviewLibrary.ViewModels
{
    /// <summary>
    /// I can see that there are diffetent responsibilities in this ViewModel, 
    /// such as managing interview data retrieval, caching, and UI state for both 
    /// interview transcripts and training content.
    /// this needs to be split into at least two different view models, one for the interview and one for the training,
    /// </summary>
    public interface IInterviewLibraryViewModel : IAsyncInitialization
    {
        /// <summary>
        /// Retrieves paginated interviews for the current user with optional company filtering.
        /// </summary>
        ValueTask<GridItemsProviderResult<InterviewDTO>> GetInterviews(GridItemsProviderRequest<InterviewDTO> request);
        
        /// <summary>
        /// Company name filter for interview search.
        /// </summary>
        string CompanySearch { get; set; }
        
        /// <summary>
        /// Pagination state for the interview grid.
        /// </summary>
        PaginationState Pagination { get; }
        
        /// <summary>
        /// Returns the CSS class for a row based on selection state.
        /// </summary>
        string? GetRowCssClass(InterviewDTO jobPost);

        /// <summary>
        /// Interview message history/transcript for the selected interview.
        /// </summary>
        List<ChatMessage> History { get; set; }

        /// <summary>
        /// Training analysis content for the selected interview.
        /// </summary>
        InterviewTrainingAnalysisResultDTO? TrainingAnalysis { get; set; }

        /// <summary>
        /// Indicates whether the interview view is currently active.
        /// </summary>
        bool IsInterview { get; set; }

        /// <summary>
        /// Indicates whether the training view is currently active.
        /// </summary>
        bool IsTraining { get; set; }

        /// <summary>
        /// Loads and displays an interview's transcript from cache or protected storage.
        /// </summary>
        Task SelectInterviewAsync(InterviewDTO? args);

        /// <summary>
        /// Loads and displays training content for an interview.
        /// </summary>
        Task LoadTraining(InterviewDTO? interview);

        /// <summary>
        /// Event fired when the ViewModel state changes and the UI should re-render.
        /// </summary>
        event Action? OnRequestUIUpdate;

        bool IsLoading { get; set; }

        bool? HasWeaknesses { get; set; }

        List<EvaluationResult> Evaluations { get; set; }
    }
}
