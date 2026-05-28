using JobBank.Management;
using JobBank.Management.Abstraction;
using JobBank.Management.Interview;
using JobBank.ModelsDTO;
using JobBank.Services.Abstraction;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text.Json;

namespace WORKSCommons.Tests
{    
    public class WCTestBase
    {
        protected Mock<IServiceScopeFactory> m_MockServiceScopeFactory;
        protected Mock<ILogger<ITrainerAssistantManager>> m_MockLogger;

        protected const string UserId = "abc123";
        protected const int InterviewId = 1;

        protected const int No_Interview_Found = 2;
        protected const int Interview_With_No_Result = 3;
        protected const int Interview_With_MalFormed_Result = 4;
        protected const int Interview_With_Wrong_Result_Schema = 5;
        protected const int Interview_With_No_WeakAreas_Semantic_Validation = 6;
        protected const int Interview_With_No_Topics_Semantic_Validation = 7;
        protected const int Interview_With_No_Evalluation_Semantic_Validation = 8;
        protected const int Interview_With_No_Failed_Evaluation_Semantic_Validation = 9;

        protected void InitializeMocks()
        {
            m_MockLogger = new Mock<ILogger<ITrainerAssistantManager>>();
            m_MockLogger.Setup(x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()))
                .Verifiable();

            Mock<ITrainerAssistant> mockTrainerAssistant = new Mock<ITrainerAssistant>();
            mockTrainerAssistant.Setup(m => m.RunLLMAnalysis(It.IsAny<TrainerAnalysisMetadataDTO>(), It.IsAny<string?>(), It.IsAny<string?>()))
                .ReturnsAsync(new InterviewTrainingAnalysisResultDTO
                {
                    Version = "v1",
                    Model = "gpt-4",
                    Prompt = "Test prompt",
                    Training = new List<TrainingResultDTO>()
                    {
                    new TrainingResultDTO
                    {
                        TrainingTopic = "C# Basics",
                        Prerequisites = new List<Prerequisite>
                        {
                            new Prerequisite { ReferenceTitle = "Basic programming knowledge" }
                        },
                        TrainingSource = "https://example.com/csharp-basics",
                        TrainingType = "Video",
                        Abstract = "An introduction to C# programming language.",
                        WhereToFocus = new List<string> { "Syntax", "Data Types", "Control Structures" },
                        HomeworkQuestions = new List<string> { "What is a class in C#?", "Explain the concept of inheritance." },
                        MasteryTask = new MasteryTask { Topic = "Build a simple console application in C#" }
                    },
                    new TrainingResultDTO
                    {
                        TrainingTopic = "Advanced C#",
                        Prerequisites = new List<Prerequisite>
                        {
                            new Prerequisite { ReferenceTitle = "C# Basics" }
                        },
                        TrainingSource = "https://example.com/advanced-csharp",
                        TrainingType = "Article",
                        Abstract = "Deep dive into advanced features of C#.",
                        WhereToFocus = new List<string> { "LINQ", "Asynchronous Programming", "Design Patterns" },
                        HomeworkQuestions = new List<string> { "What is LINQ and how is it used?", "Explain async/await in C#." },
                        MasteryTask = new MasteryTask { Topic = "Create a web API using ASP.NET Core" }
                    }
                    }
                });

            Mock<ITrainingService> mockTrainingService = new Mock<ITrainingService>();
            mockTrainingService.Setup(m => m.AddTrainingAsync(It.IsAny<TrainingDTO>())).Returns((TrainingDTO trainingDto) => 
            {
                trainingDto.Id = new Random().Next(1, 1000); // Simulate database-generated ID
                return Task.CompletedTask;
            } );

            Mock<ISkillsService> mockSkillService = new Mock<ISkillsService>();
            mockSkillService.Setup(m => m.GetUserSkillsAsync(It.IsAny<string>())).ReturnsAsync((string userId) =>
            userId switch
            {
                UserId => new UserSkillsDTO
                {
                    UserId = UserId,
                    RawSkills = "C#, OOP, ASP.NET"
                },
                _ => null
            });

            var interviewMetadata = new InterviewMetadata
            {
                CoveredTopics = new List<string> { "C# Basics", "Advanced C#" },
                WeakAreas = new List<string> { "Asynchronous Programming" },
                Evaluations = new List<EvaluationResult>
            {
                new EvaluationResult
                {
                    PreviousQuestion = "What is a class in C#?",
                    PreviousTopic = "C# Basics",
                    Score = 5,
                    Weight = 5,
                    Passed = true,
                    Strengths = new List<string> { "Good understanding of OOP concepts" },
                    Gaps = new List<string> { "Needs to improve on asynchronous programming" },
                    Confidence = 0.9
                },
                new EvaluationResult
                {
                    PreviousQuestion = "Explain the concept of inheritance.",
                    PreviousTopic = "Advanced C#",
                    Score = 4,
                    Weight = 5,
                    Passed = true,
                    Strengths = new List<string> { "Able to explain inheritance clearly" },
                    Gaps = new List<string> { "Could provide more examples" },
                    Confidence = 0.8
                },
                new EvaluationResult
                {
                    PreviousQuestion = "What is LINQ and how is it used?",
                    PreviousTopic = "Expression Trees",
                    Score = 3,
                    Weight = 5,
                    Passed = false,
                    Strengths = new List<string> { "Understands basic LINQ queries" },
                    Gaps = new List<string> { "Needs to improve understanding of LINQ" },
                    Confidence = 0.6
                }
            }
            };

            string result = JsonSerializer.Serialize<InterviewMetadata>(interviewMetadata);

            Mock<IInterviewService> mockInterviewService = new Mock<IInterviewService>();
            mockInterviewService.Setup(m => m.GetInterviewByIdAsync(It.IsAny<int>())).ReturnsAsync((int interviewId) =>
            interviewId switch
            {
                InterviewId => new InterviewDTO
                {
                    Id = 1,
                    UserId = "user123",
                    JobPostId = 101,
                    CompletedAtUtc = DateTime.UtcNow.AddDays(-7),
                    IsDeleted = false,
                    Result = result,
                },
                Interview_With_No_Result => new InterviewDTO
                {
                    Id = 2,
                    UserId = "user123",
                    JobPostId = 101,
                    CompletedAtUtc = DateTime.UtcNow.AddDays(-7),
                    IsDeleted = false,
                    Result = null,
                },
                Interview_With_MalFormed_Result => new InterviewDTO
                {
                    Id = 3,
                    UserId = "user123",
                    JobPostId = 101,
                    CompletedAtUtc = DateTime.UtcNow.AddDays(-7),
                    IsDeleted = false,
                    Result = "This is not a valid JSON string",
                },
                Interview_With_Wrong_Result_Schema => new InterviewDTO
                {
                    Id = 4,
                    UserId = "user123",
                    JobPostId = 101,
                    CompletedAtUtc = DateTime.UtcNow.AddDays(-7),
                    IsDeleted = false,
                    Result = JsonSerializer.Serialize(new { WrongProperty = "This does not match the expected schema" }),
                },
                Interview_With_No_WeakAreas_Semantic_Validation =>
                new InterviewDTO
                {
                    Id = 5,
                    UserId = "user123",
                    JobPostId = 101,
                    CompletedAtUtc = DateTime.UtcNow.AddDays(-7),
                    IsDeleted = false,
                    Result = JsonSerializer.Serialize(new InterviewMetadata
                    {
                        CoveredTopics = new List<string> { "C# Basics" },
                        WeakAreas = new List<string>(),
                        Evaluations = new List<EvaluationResult>
                        {
                        new EvaluationResult
                        {
                            PreviousQuestion = "What is a class in C#?",
                            PreviousTopic = "C# Basics",
                            Score = 5,
                            Weight = 5,
                            Passed = false,
                            Strengths = new List<string> { "Good understanding of OOP concepts" },
                            Gaps = new List<string> { "Needs to improve on asynchronous programming" },
                            Confidence = 0.9
                        }
                        }
                    }),
                },
                Interview_With_No_Topics_Semantic_Validation =>
                new InterviewDTO
                {
                    Id = 5,
                    UserId = "user123",
                    JobPostId = 101,
                    CompletedAtUtc = DateTime.UtcNow.AddDays(-7),
                    IsDeleted = false,
                    Result = JsonSerializer.Serialize(new InterviewMetadata
                    {
                        CoveredTopics = new List<string>(),
                        WeakAreas = new List<string> { "OOP", "Inheritance", "asynchronous programming" },
                        Evaluations = new List<EvaluationResult>
                        {
                        new EvaluationResult
                        {
                            PreviousQuestion = "What is a class in C#?",
                            PreviousTopic = "C# Basics",
                            Score = 0.3,
                            Weight = 5,
                            Passed = false,
                            Strengths = new List<string> { "Poor understanding of OOP concepts" },
                            Gaps = new List<string> { "Needs to improve on asynchronous programming" },
                            Confidence = 0.9
                        }
                        }
                    }),
                },
                Interview_With_No_Evalluation_Semantic_Validation =>
                new InterviewDTO
                {
                    Id = 5,
                    UserId = "user123",
                    JobPostId = 101,
                    CompletedAtUtc = DateTime.UtcNow.AddDays(-7),
                    IsDeleted = false,
                    Result = JsonSerializer.Serialize(new InterviewMetadata
                    {
                        CoveredTopics = new List<string> { "C# Basics" },
                        WeakAreas = new List<string> { "OOP", "Inheritance", "asynchronous programming" },
                        Evaluations = new List<EvaluationResult>()
                    }),
                },
                Interview_With_No_Failed_Evaluation_Semantic_Validation =>
                new InterviewDTO
                {
                    Id = 5,
                    UserId = "user123",
                    JobPostId = 101,
                    CompletedAtUtc = DateTime.UtcNow.AddDays(-7),
                    IsDeleted = false,
                    Result = JsonSerializer.Serialize(new InterviewMetadata
                    {
                        CoveredTopics = new List<string> { "C# Basics" },
                        WeakAreas = new List<string> { "OOP", "Inheritance", "asynchronous programming" },
                        Evaluations = new List<EvaluationResult>
                        {
                        new EvaluationResult
                        {
                            PreviousQuestion = "What is a class in C#?",
                            PreviousTopic = "C# Basics",
                            Score = 5,
                            Weight = 5,
                            Passed = true,
                            Strengths = new List<string> { "Good understanding of OOP concepts" },
                            Gaps = new List<string> { "Needs to improve on asynchronous programming" },
                            Confidence = 0.9
                        }
                        }
                    }),
                },
                No_Interview_Found => null,
                _ => null
            });

            Mock<IJobPostService> mockJobPostService = new Mock<IJobPostService>();
            mockJobPostService.Setup(m => m.GetJobPostByIdAsync(It.IsAny<int>())).ReturnsAsync(new JobPostDTO
            {
                Id = 101,
                Title = "Software Engineer",
                Company = "Tech Company",
                Description = "We are looking for a skilled Software Engineer with experience in C# and ASP.NET."
            });

            m_MockServiceScopeFactory = new Mock<IServiceScopeFactory>();
            var serviceProvider = new ServiceCollection()
                .AddSingleton(m_MockLogger.Object)
                .AddSingleton(mockTrainerAssistant.Object)
                .AddSingleton(mockTrainingService.Object)
                .AddSingleton(mockSkillService.Object)
                .AddSingleton(mockInterviewService.Object)
                .AddSingleton(mockJobPostService.Object)
                .AddSingleton<JobDescriptionParser>()
                .BuildServiceProvider();
            m_MockServiceScopeFactory.Setup(m => m.CreateScope()).Returns(serviceProvider.CreateScope());
        }
    }
}