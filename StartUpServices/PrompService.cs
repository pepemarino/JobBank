using System.Security.Policy;

namespace JobBank.StartUpServices
{
    public class PrompService
    {
        public string InterviewQuestions { get; set; } = string.Empty;
        public string SkillLLMMatchPercentage { get; set; } = string.Empty;

        /// <summary>
        /// This timeout is used to prevent the system from waiting indefinitely for a 
        /// response from the LLM, which could lead to a poor user experience. 
        /// 45 seconds is a reasonable default, but it can be adjusted based on 
        /// the expected response times of the LLM and the needs of the application.
        /// Very Important: this is used in a cancellation token for the async call to the LLM, 
        /// if set to zero then the cancellation token will signal immediately and the LLM 
        /// call will be cancelled right away, resulting in no response from the LLM.
        /// Default is 45 seconds, which is a reasonable balance between giving the 
        /// LLM enough time to respond and not keeping the user waiting too long.
        /// </summary>
        public long TimeoutSeconds { get; set; } = 45;

        public string ApiKeyName { get; set; } = string.Empty;
        public string LLMModel { get; set; } = string.Empty;
        public string SkillGap {  get; set; } = string.Empty;
    }
}
