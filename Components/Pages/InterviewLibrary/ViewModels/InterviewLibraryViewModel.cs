using JobBank.Management.Interview;
using JobBank.ModelsDTO;
using JobBank.Services.Abstraction;
using Microsoft.AspNetCore.Components.QuickGrid;
using System.Text.Json;

namespace JobBank.Components.Pages.InterviewLibrary.ViewModels
{
    public class InterviewLibraryViewModel : IInterviewLibraryViewModel
    {
        private readonly IProtectedLocalStoreService<List<ChatMessage>> _interviewMessagesStore;
        private readonly ILogger<InterviewLibraryViewModel> _logger;
        private readonly IIdentityService _identityService;
        private readonly IInterviewService _interviewService;
        private readonly ITrainingService _trainingService;
        private InterviewDTO selected;
        private readonly Dictionary<string, List<ChatMessage>> _historyCache = new(); // cache transcripts

        public InterviewLibraryViewModel(
            ILogger<InterviewLibraryViewModel> logger,
            IIdentityService identityService,
            IInterviewService interviewService,
            IProtectedLocalStoreService<List<ChatMessage>> interviewMessagesStore,
            ITrainingService trainingService)
        {
            _logger = logger;
            _identityService = identityService;
            _interviewService = interviewService;
            _interviewMessagesStore = interviewMessagesStore;
            _trainingService = trainingService;
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
            return selected != null && selected.Equals(interview) ? "selected-row" : null;
        }

        public async Task SelectInterviewAsync(InterviewDTO args)
        {
            selected = args;
            IsInterview = true;
            IsTraining = !IsInterview;

            var historyKey = $"interview-messages-{args.Id}-{args.JobPostId}";
            
            try
            {                
                if (_historyCache.TryGetValue(historyKey, out var cachedHistory))
                {
                    History = cachedHistory;
                }
                else
                {
                    var loadedHistory = await _interviewMessagesStore.LoadAsync(historyKey) ?? new List<ChatMessage>();
                    _historyCache[historyKey] = loadedHistory;
                    History = loadedHistory;
                }
                
                OnRequestUIUpdate?.Invoke();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load interview messages for interview ID {InterviewId}", args.Id);
                History = new List<ChatMessage>();
            }
        }

        public async Task InitializeAsync()
        {
           OnRequestUIUpdate?.Invoke();            
        }

        public InterviewTrainingAnalysisResultDTO TrainingAnalysys { get; set; }

        public async Task LoadTraining(InterviewDTO interview)
        {
            selected = interview;
            IsTraining = true;
            IsInterview = !IsTraining;
            
            var training = await _trainingService.GetTrainingByIdAsync(interview.TrainingId);
            if (training != null && !string.IsNullOrEmpty(training.Result))
            {
                TrainingAnalysys = JsonSerializer.Deserialize<InterviewTrainingAnalysisResultDTO>(training!.Result);
            }

            OnRequestUIUpdate?.Invoke();
        }

        public bool IsInterview { get; set; }
        public bool IsTraining { get; set; }
    }
}
