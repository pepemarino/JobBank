using JobBank.Management.Interview;
using JobBank.ModelsDTO;
using JobBank.Services.Abstraction;
using Microsoft.AspNetCore.Components.QuickGrid;

namespace JobBank.Components.Pages.InterviewLibrary.ViewModels
{
    public class InterviewLibraryViewModel : IInterviewLibraryViewModel
    {
        private readonly ILogger<InterviewLibraryViewModel> _logger;
        private readonly IIdentityService _identityService;
        private readonly IInterviewService _interviewService;

        public InterviewLibraryViewModel(
            ILogger<InterviewLibraryViewModel> logger,
            IIdentityService identityService,
            IInterviewService interviewService)
        {
            _logger = logger;
            _identityService = identityService;
            _interviewService = interviewService;
        }

        public string CompanySearch { get; set; } = string.Empty;

        public PaginationState Pagination { get; } = new() { ItemsPerPage = 10 };

        public List<ChatMessage> History { get; set; } = new();

        private string _userId = string.Empty;

        private async Task<string> GetUserIdAsync()
        {
            if (string.IsNullOrEmpty(_userId))
            {
                _userId = await _identityService.GetUserIdAsync();
            }
            return _userId;
        }

        public async ValueTask<GridItemsProviderResult<InterviewDTO>> GetInterviews(GridItemsProviderRequest<InterviewDTO> request)
        {
            var user = await GetUserIdAsync();
            
            var paginationResult = await _interviewService.GetInterviewsByUserIdWithPaginationAsync(
                user, 
                CompanySearch, 
                request.StartIndex, 
                request.Count ?? 10);

            return GridItemsProviderResult.From(paginationResult.Items.ToList(), paginationResult.TotalCount);
        }

        public string GetRowCssClass(InterviewDTO interview)
        {
            if (!interview.Passed)
            {
                return "declined-row";
            }

            return "applied-row";
        }
    }
}
