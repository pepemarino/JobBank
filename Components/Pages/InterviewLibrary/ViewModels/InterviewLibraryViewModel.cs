using JobBank.Management.Interview;
using JobBank.ModelsDTO;
using JobBank.Services.Abstraction;
using Microsoft.AspNetCore.Components.QuickGrid;

namespace JobBank.Components.Pages.InterviewLibrary.ViewModels
{
    public class InterviewLibraryViewModel : IInterviewLibraryViewModel
    {
        private readonly IProtectedLocalStoreService<List<ChatMessage>> _interviewMessagesStore;
        private readonly ILogger<InterviewLibraryViewModel> _logger;
        private readonly IIdentityService _identityService;
        private readonly IInterviewService _interviewService;
        private InterviewDTO selected;

        public InterviewLibraryViewModel(
            ILogger<InterviewLibraryViewModel> logger,
            IIdentityService identityService,
            IInterviewService interviewService,
            IProtectedLocalStoreService<List<ChatMessage>> interviewMessagesStore)
        {
            _logger = logger;
            _identityService = identityService;
            _interviewService = interviewService;
            _interviewMessagesStore = interviewMessagesStore;
        }

        public string CompanySearch { get; set; } = string.Empty;

        public PaginationState Pagination { get; } = new() { ItemsPerPage = 10 };

        public List<ChatMessage> History { get; set; } = new();

        private string _userId = string.Empty;

        public event Action? OnRequestUIUpdate;

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

        public string? GetRowCssClass(InterviewDTO interview)
        {
            return selected != null && selected.Equals(interview) ? "table-primary fw-bold" : null;
        }

        public async Task SelectInterviewAsync(InterviewDTO args)
        {
            selected = args;
            var historyKey = $"interview-messages-{args.Id}-{args.JobPostId}";
            try
            {
                History = await _interviewMessagesStore.LoadAsync(historyKey) ?? new List<ChatMessage>();
                OnRequestUIUpdate?.Invoke();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load interview messages for interview ID {InterviewId}", args.Id);
                History = new List<ChatMessage>();
            }

            Console.WriteLine($"Selected interview with ID: {args.Id}");
        }

        public async Task InitializeAsync()
        {
           OnRequestUIUpdate?.Invoke();            
        }
    }
}
