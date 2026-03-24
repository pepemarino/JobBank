namespace JobBank.Management
{
    using JobBank.Management.Abstraction;
    using JobBank.ModelsDTO;
    using JobBank.StartUpServices;
    using System.Text.Json.Serialization;

    public record LLMAnalysisResult(        
        string Version,               // Required
        string Model,                 // Required
        string Analysis = "Analysis Error",
        int? JobApplicationId = null, 
        string? UserId = null,                    
        string? Prompt = null,
        string? ErrorMessage = null
    );

    public partial class CareerAssistant
    {
        private readonly string _version = "v1";
        private readonly string _llmModel;        
        private readonly long _timeout;
        private string _apiKey; // Store

        private readonly ILLMManager _llmManager;

        public CareerAssistant(PrompService prompService, ILLMManager llmManager)
        {
            _timeout = prompService.TimeoutSeconds;
            _llmModel = prompService.LLMModel;            
            _llmManager = llmManager;          
        }

        private async Task<bool> CanAnalyseAsync()
        {
            return await _llmManager.IsAvailableAsync();            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="subjectDescription"></param>
        /// <param name="prompt"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<LLMAnalysisResult> RunLLMAnalysis(string subjectDescription, string prompt, string? userId = null)
        {
            var canAnalyse = await _llmManager.IsAvailableAsync(userId);
            if (!canAnalyse)
                return new(ErrorMessage: "LLM analysis is not enabled. API key is missing.", Version: _version, Model: _llmModel);

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(_timeout));

            try
            {
                return await Analyze(subjectDescription, prompt, userId, cts.Token);
            }
            catch (OperationCanceledException)
            {
                return new(ErrorMessage: "Operation was cancelled by the system due to timeout.", Version: _version, Model: _llmModel);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="analysisDTO"></param>
        /// <param name="prompt"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<JobApplicationAnalysisDTO> RunLLMAnalysis(JobApplicationAnalysisDTO analysisDTO, string prompt, string? userId = null)
        {
            var canAnalyse = await _llmManager.IsAvailableAsync(userId);
            if (!canAnalyse)
            {
                analysisDTO.AnalysisResult = "LLM analysis is not enabled. API key is missing.";
                return analysisDTO;
            }

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(_timeout));
            try
            {
                return await Analyze(analysisDTO, prompt, userId, cts.Token);
            }
            catch (OperationCanceledException ex)
            {
                throw;
            }
        }


    }

    // Data model for structured response
    public class AnalysisResult
    {
        [JsonPropertyName("InterviewQuestions")]
        public List<string> InterviewQuestions { get; set; } = new();

        [JsonPropertyName("StudySubjects")]
        public List<string> StudySubjects { get; set; } = new();
    }
}
