using JobBank.Extensions;
using JobBank.Management.Abstraction;
using JobBank.Management.Interview;
using JobBank.ModelsDTO;
using JobBank.Services.Abstraction;
using JobBank.Util;
using Microsoft.AspNetCore.Components.QuickGrid;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace JobBank.Components.Pages.InterviewLibrary.ViewModels
{
    /// <summary>
    /// ViewModel for managing the Interview Library page, handling interview transcript display and training content.
    /// Implements caching for both interview messages and training data to optimize performance.
    /// </summary>
    public class InterviewLibraryViewModel : IInterviewLibraryViewModel
    {
        private readonly IProtectedLocalStoreService<List<ChatMessage>> _interviewMessagesStore;
        private readonly ILogger<InterviewLibraryViewModel> _logger;
        private readonly IIdentityService _identityService;
        private readonly IInterviewService _interviewService;
        private readonly ITrainingService _trainingService;
        private readonly ITrainerAssistantManager _trainerAssistantManager;

        private InterviewDTO? _selectedInterview;
        private string _userId = string.Empty;

        // Cache configurations
        private const int MaxCacheSize = 50; // Prevent unbounded memory growth
        private readonly Dictionary<string, List<ChatMessage>> _historyCache = new();
        private readonly Dictionary<int, InterviewTrainingAnalysisResultDTO> _trainingCache = new();

        public InterviewLibraryViewModel(
            ILogger<InterviewLibraryViewModel> logger,
            IIdentityService identityService,
            IInterviewService interviewService,
            IProtectedLocalStoreService<List<ChatMessage>> interviewMessagesStore,
            ITrainingService trainingService,
            ITrainerAssistantManager trainerAssistantManager)
        {
            _logger = logger;
            _identityService = identityService;
            _interviewService = interviewService;
            _interviewMessagesStore = interviewMessagesStore;
            _trainingService = trainingService;
            _trainerAssistantManager = trainerAssistantManager;
        }

        public string CompanySearch { get; set; } = string.Empty;

        public PaginationState Pagination { get; } = new() { ItemsPerPage = 10 };

        public List<ChatMessage> History { get; set; } = new();

        public InterviewTrainingAnalysisResultDTO? TrainingAnalysis { get; set; }

        public bool IsInterview { get; set; }

        public bool IsTraining { get; set; }
        public bool IsLoading { get; set; }
        public bool? HasWeaknesses { get; set; }

        public List<EvaluationResult> Evaluations { get; set; } = new();

        public event Action? OnRequestUIUpdate;

        /// <summary>
        /// Gets the current user ID. Note it is the Task what is cached, not the result, 
        /// to ensure that we don't have issues with stale user IDs if the user logs out 
        /// and another user logs in within the same session. Caching the Task allows us 
        /// to await the same Task for multiple calls while it's still running, and once 
        /// it completes, all callers will get the same result. If we cached the result 
        /// directly, we might end up with a stale user ID if the user changes during the session. 
        /// By caching the Task, we ensure that we always get the correct user ID for 
        /// the current session without risking stale data.
        /// </summary>
        private Task<string>? _userIdTask;
        private async Task<string> GetUserIdAsync()
        {
            _userIdTask ??= _identityService.GetUserIdAsync();
            return await _userIdTask;
        }

        /// <summary>
        /// Retrieves paginated interviews for the current user with optional company filtering.
        /// </summary>
        public async ValueTask<GridItemsProviderResult<InterviewDTO>> GetInterviews(GridItemsProviderRequest<InterviewDTO> request)
        {
            try
            {
                var user = await GetUserIdAsync();

                var paginationResult = await _interviewService.GetInterviewsByUserIdWithPaginationAsync(
                    user,
                    CompanySearch,
                    request.StartIndex,
                    request.Count ?? 10);

                return GridItemsProviderResult.From(paginationResult.Items.ToList(), paginationResult.TotalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving interviews for pagination");
                return GridItemsProviderResult.From(new List<InterviewDTO>(), 0);
            }
        }

        /// <summary>
        /// Gets the CSS class for a row based on selection state.
        /// </summary>
        public string? GetRowCssClass(InterviewDTO interview)
        {
            return _selectedInterview != null && _selectedInterview.Equals(interview) ? "selected-row" : null;
        }

        /// <summary>
        /// Loads and displays an interview's transcript from cache or protected storage.
        /// </summary>
        public async Task SelectInterviewAsync(InterviewDTO? args)
        {
            if (args == null)
            {
                _logger.LogWarning("SelectInterviewAsync called with null interview");
                return;
            }

            _selectedInterview = args;
            IsInterview = true;
            IsTraining = false;

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

                    // Manage cache size to prevent memory issues
                    if (_historyCache.Count >= MaxCacheSize)
                    {
                        _historyCache.Remove(_historyCache.Keys.First());
                    }

                    _historyCache[historyKey] = loadedHistory;
                    History = loadedHistory;
                }

                OnRequestUIUpdate?.Invoke();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load interview messages for interview ID {InterviewId}", args.Id);
                History = new List<ChatMessage>();
                IsInterview = false;
            }
        }

        /// <summary>
        /// Loads and displays training content for an interview.
        /// Training data is cached to avoid repeated database queries.
        /// </summary>
        public async Task LoadTraining(InterviewDTO? interview)
        {
            if (interview == null)
            {
                _logger.LogWarning("LoadTraining called with null or invalid interview");
                return;
            }

            _selectedInterview = interview;
            IsLoading = false;
            IsTraining = true;
            IsInterview = false;

            TrainingAnalysis = null;
            Evaluations = new List<EvaluationResult>();

            try
            {
                // Check cache first
                if (_trainingCache.TryGetValue(interview.TrainingId, out var cachedTraining))
                {
                    TrainingAnalysis = cachedTraining;
                }
                else
                {
                    if (interview.TrainingId <= 0)
                    {
                        if (NeedsTraining(interview))
                        {
                            _logger.LogWarning("LoadTraining for interview ID {InterviewId} does not exist", interview.Id);
                            IsLoading = true;
                            interview.TrainingId = await _trainerAssistantManager.AnalyzeInterviewAsync(userId: await GetUserIdAsync(), interviewId: interview.Id);
                            if (interview.TrainingId <= 0)
                            {
                                _logger.LogError("Failed to analyze interview ID {InterviewId} and get valid TrainingId", interview.Id);
                                return;
                            }
                        }
                        else
                        {
                            _logger.LogWarning("Interview ID {InterviewId} does not require training or failed to generate training", interview.Id);
                            return;
                        }
                    }

                    var training = await _trainingService.GetTrainingByIdAsync(interview.TrainingId);

                    if (training != null && !string.IsNullOrEmpty(training.Result))
                    {
                        TrainingAnalysis = JsonSerializer.Deserialize<InterviewTrainingAnalysisResultDTO>(training.Result);

                        // Manage cache size
                        if (_trainingCache.Count >= MaxCacheSize)
                        {
                            _trainingCache.Remove(_trainingCache.Keys.First());
                        }

                        if (TrainingAnalysis != null)
                        {
                            _trainingCache[interview.TrainingId] = TrainingAnalysis;
                        }
                    }
                    else
                    {
                        _logger.LogWarning("Training data not found for TrainingId {TrainingId}", interview.TrainingId);
                        TrainingAnalysis = null;
                    }
                }
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to deserialize training data for TrainingId {TrainingId}", interview.TrainingId);
                TrainingAnalysis = null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load training for interview ID {InterviewId}", interview.Id);
                TrainingAnalysis = null;
            }
            finally
            {
                IsLoading = false;
                OnRequestUIUpdate?.Invoke();
            }
        }

        private bool NeedsTraining(InterviewDTO interview)
        {
            if (string.IsNullOrEmpty(interview.Result))
            {
                _logger.LogWarning("NeedsTraining called with empty interview result for interview ID {InterviewId}", interview.Id);
                throw new InvalidOperationException("Interview result is empty, cannot determine training needs.");
            }
          
            if (!interview.Result.IsValidJson<InterviewMetadata>(
                    Common.StrictValidationOptions))
            {
                _logger.LogWarning("NeedsTraining called with invalid result for interview ID {InterviewId}", interview.Id);
                throw new InvalidOperationException("Interview result is not valid, cannot determine training needs.");
            }

            var interviewMetadata = JsonSerializer.Deserialize<InterviewMetadata>(interview.Result);   
            if (interviewMetadata == null)
            {
                _logger.LogWarning("NeedsTraining deserialization resulted in null for interview ID {InterviewId}", interview.Id);
                throw new InvalidOperationException("Deserialized interview metadata is null, cannot determine training needs.");
            }

            Evaluations = interviewMetadata.Evaluations;

            // the schema has already been validated, so we can assume that Evaluations is not null and contains valid data
            // If there are weak topics, as defined by the business rules, then we need training.           
            var weakTopics = BusinessRules.NeedsTrainingRule(interviewMetadata);                   
            return weakTopics.Any();
        }

        /// <summary>
        /// Initializes the ViewModel. Called by the component on initialization.
        /// </summary>
        public async Task InitializeAsync()
        {
            OnRequestUIUpdate?.Invoke();
            await Task.CompletedTask;
        }
    }
}
