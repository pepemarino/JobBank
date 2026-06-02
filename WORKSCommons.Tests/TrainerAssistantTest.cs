using JobBank.Management;

namespace WORKSCommons.Tests;

[TestClass]
public class TrainerAssistantTest : WCTestBase
{
    [TestInitialize]
    public void Setup()
    {
        InitializeMocks();
    }

    [TestMethod]
    public async Task RunLLMAnalysisArgumentExceptionOn_Empty_UsetrId_Async()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
        {
            var trainerAssistantManager = new TrainerAssistantManager(
                m_MockServiceScopeFactory.Object,
                m_MockLogger.Object);

            return trainerAssistantManager.AnalyzeInterviewAsync(string.Empty, InterviewId);
        });
    }

    [TestMethod]
    public async Task RunLLMAnalysisArgumentExceptionOn_Null_UserId_Async()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
        {
            var trainerAssistantManager = new TrainerAssistantManager(
                m_MockServiceScopeFactory.Object,
                m_MockLogger.Object);

            return trainerAssistantManager.AnalyzeInterviewAsync(null, InterviewId);
        });
    }

    [TestMethod]
    public async Task RunLLMAnalysisInvalidOperationExceptionFor_User_With_No_Skills()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
        {
            var trainerAssistantManager = new TrainerAssistantManager(
                m_MockServiceScopeFactory.Object,
                m_MockLogger.Object);

            return trainerAssistantManager.AnalyzeInterviewAsync("no_Skills", InterviewId);
        });
    }

    [TestMethod]
    public async Task RunLLMAnalysisInvalidOperationExceptionFor_No_Interview_Found()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
        {
            var trainerAssistantManager = new TrainerAssistantManager(
                m_MockServiceScopeFactory.Object,
                m_MockLogger.Object);

            return trainerAssistantManager.AnalyzeInterviewAsync(UserId, No_Interview_Found);
        });
    }

    [TestMethod]
    public async Task RunLLMAnalysisInvalidOperationExceptionFor_Interview_With_NoResult()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
        {
            var trainerAssistantManager = new TrainerAssistantManager(
                m_MockServiceScopeFactory.Object,
                m_MockLogger.Object);

            return trainerAssistantManager.AnalyzeInterviewAsync(UserId, Interview_With_No_Result);
        });
    }

    [TestMethod]
    public async Task RunLLMAnalysisInvalidOperationExceptionFor_Interview_With_Malformed_Result()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
        {
            var trainerAssistantManager = new TrainerAssistantManager(
                m_MockServiceScopeFactory.Object,
                m_MockLogger.Object);

            return trainerAssistantManager.AnalyzeInterviewAsync(UserId, Interview_With_MalFormed_Result);
        });
    }

    [TestMethod]
    public async Task RunLLMAnalysisInvalidOperationExceptionFor_Interview_With_Invalid_Result_Schema()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
        {
            var trainerAssistantManager = new TrainerAssistantManager(
                m_MockServiceScopeFactory.Object,
                m_MockLogger.Object);

            return trainerAssistantManager.AnalyzeInterviewAsync(UserId, Interview_With_Wrong_Result_Schema);
        });
    }

    [TestMethod]
    public async Task RunLLMAnalysisInvalidOperationExceptionForInterviewWith_Interview_With_No_Evalluation_Semantic_Validation()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
        {
            var trainerAssistantManager = new TrainerAssistantManager(
                m_MockServiceScopeFactory.Object,
                m_MockLogger.Object);

            return trainerAssistantManager.AnalyzeInterviewAsync(UserId, Interview_With_No_Evalluation_Semantic_Validation);
        });
    }

    [TestMethod]
    public async Task RunLLMAnalysisInvalidOperationExceptionForInterviewWith_Interview_With_No_Failed_Evaluation_Semantic_Validation()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
        {
            var trainerAssistantManager = new TrainerAssistantManager(
                m_MockServiceScopeFactory.Object,
                m_MockLogger.Object);

            return trainerAssistantManager.AnalyzeInterviewAsync(UserId, Interview_With_No_Failed_Evaluation_Semantic_Validation);
        });
    }

    [TestMethod]
    public async Task RunLLMAnalysisInvalidOperationExceptionFor_No_JobPost_Found()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
        {
            var trainerAssistantManager = new TrainerAssistantManager(
                m_MockServiceScopeFactory.Object,
                m_MockLogger.Object);

            return trainerAssistantManager.AnalyzeInterviewAsync(UserId, 10);
        });
    }

    [TestMethod]
    public async Task RunLLMAnalysisInvalidOperationExceptionFor_Empty_JobDescription()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
        {
            var trainerAssistantManager = new TrainerAssistantManager(
                m_MockServiceScopeFactory.Object,
                m_MockLogger.Object);

            return trainerAssistantManager.AnalyzeInterviewAsync(UserId, 11);
        });
    }

    [TestMethod]
    public async Task RunLLMAnalysisInvalidOperationExceptionFor_Failed_JobDescription_Parse()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
        {
            var trainerAssistantManager = new TrainerAssistantManager(
                m_MockServiceScopeFactory.Object,
                m_MockLogger.Object);

            return trainerAssistantManager.AnalyzeInterviewAsync(UserId, 12);
        });
    }

    [TestMethod]
    public async Task RunLLMAnalysisInvalidOperationExceptionFor_LLM_Analysis_Error()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
        {
            var trainerAssistantManager = new TrainerAssistantManager(
                m_MockServiceScopeFactory.Object,
                m_MockLogger.Object);

            return trainerAssistantManager.AnalyzeInterviewAsync(UserId, 13);
        });
    }

    [TestMethod]
    public async Task RunLLMAnalysisSuccess()
    {
        var trainerAssistantManager = new TrainerAssistantManager(
            m_MockServiceScopeFactory.Object,
            m_MockLogger.Object);
        int result = await trainerAssistantManager.AnalyzeInterviewAsync(UserId, InterviewId);
        Assert.IsTrue(result > 0);
    }

    [TestMethod]
    public async Task RunLLMAnalysisSuccessWithCustomPrompt()
    {
        var trainerAssistantManager = new TrainerAssistantManager(
            m_MockServiceScopeFactory.Object,
            m_MockLogger.Object);

        string customPrompt = "Custom analysis prompt";
        int result = await trainerAssistantManager.AnalyzeInterviewAsync(UserId, InterviewId, customPrompt);

        Assert.IsTrue(result > 0);
    }
}
