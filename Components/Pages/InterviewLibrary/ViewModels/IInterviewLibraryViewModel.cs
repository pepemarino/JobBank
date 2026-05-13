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

        Task SelectInterviewAsync(InterviewDTO args);

        public event Action? OnRequestUIUpdate;
    }
}
