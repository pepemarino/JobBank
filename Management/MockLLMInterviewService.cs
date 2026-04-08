using JobBank.Management.Abstraction;
using JobBank.Management.Interview;
using static JobBank.Management.Abstraction.IInterviewLLMService;

namespace JobBank.Management
{
    public class MockLLMInterviewService : IInterviewLLMService
    {
        private readonly List<(string Question, string Topic, int Weight)> questions = new()
        {
            ("Can you tell me about yourself?", "General", 1),
            ("Can you be specific about your personal experience and provide an example?", "General", 1),
            ("What interests you about this role?", "Motivation", 3),
            ("Do you find the possition interesting?", "Motivation", 3),
            ("Explain what an ORM is.", "Data Access", 5),
            ("Explain with an example how have you used or design a Data Access Layer.", "Data Access", 5),
            ("Difference between abstract class and interface?", "OOP", 10),
            ("Can you explain polimorphism?", "OOP", 10),
            ("How do you handle multiple deadlines?", "Productivity", 10),
            ("Describe a challenging team project.", "Collaboration",5),
        };

        public async Task<InterviewerLLMDTO> GetInterviewerAnalysisAsync(
            UserJobApplicantDTO userDTO,
            string prompt)
        {
            EvaluationResult? evaluation = null;

            // Evaluate previous answer
            if (!string.IsNullOrWhiteSpace(userDTO.UserAnswer))
            {
                evaluation = EvaluateAnswer(
                    userDTO.History.LastOrDefault(m => m.Role == InterviewRole.Interviewer.ToString())?.Content ?? "",
                    userDTO.QuestionTopic,
                    userDTO.UserAnswer
                );

                if(!userDTO.Evaluations.Any(e => e.Equals(evaluation)))
                {
                    userDTO.Evaluations.Add(evaluation);
                }
                
                // Update WeakAreas
                foreach (var gap in evaluation.Gaps)
                {
                    if (!userDTO.WeakAreas.Contains(gap))
                        userDTO.WeakAreas.Add(gap);
                }
            }

            // Pick next question
            var (question, topic, weight) = GetNextQuestion(userDTO);

            // Update CoveredTopics
            if (!userDTO.CoveredTopics.Contains(topic))
                userDTO.CoveredTopics.Add(topic);

            return new InterviewerLLMDTO
            {
                AgentQuestion = question,
                QuestionTopic = topic,
                Evaluation = evaluation,
                CoveredTopics = userDTO.CoveredTopics,
                WeakAreas = userDTO.WeakAreas
            };
        }

        /// <summary>
        /// This gives a little flavor of state driven
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        private (string Question, string Topic, int Weight) GetNextQuestion(UserJobApplicantDTO user)
        {
            // Prioritize weak areas
            var weakTopic = user.WeakAreas.FirstOrDefault();

            if (!string.IsNullOrEmpty(weakTopic))
            {
                var match = questions
                    .FirstOrDefault(q => q.Topic == weakTopic &&
                                         user.History.All(m => m.Content != q.Question));

                if (match != default)
                    return match;
            }

            // Otherwise pick uncovered topic
            var next = questions
                .FirstOrDefault(q => !user.CoveredTopics.Contains(q.Topic) &&
                                      user.History.All(m => m.Content != q.Question));

            if (next != default)
                return next;

            // fallback (all covered)
            return questions[Random.Shared.Next(questions.Count)];
        }

        /// <summary>
        /// it provides deterministic-ish behavior, topic-driven weaknesses, something the Trainer can later use
        /// </summary>
        /// <param name="question"></param>
        /// <param name="topic"></param>
        /// <param name="answer"></param>
        /// <returns></returns>
        private EvaluationResult EvaluateAnswer(string question, string topic, string answer)
        {
            var lengthScore = Math.Min(answer.Length / 200.0, 1.0); // crude signal
            var randomFactor = Random.Shared.NextDouble() * 0.3;

            var weight = questions.FirstOrDefault(q => q.Question == question).Weight;

            var score = Math.Min(lengthScore + randomFactor, 1.0);
            var passed = score >= 0.6;

            var gaps = new List<string>();
            var strengths = new List<string>();

            if (passed)
            {
                strengths.Add("Clear communication");
                if (score > 0.8)
                    strengths.Add("Good depth");
            }
            else
            {
                gaps.Add(topic); // important: tie gap to topic
            }

            return new EvaluationResult
            {
                PreviousQuestion = question,
                PreviousTopic = topic,
                Score = score,
                Weight = weight,
                Passed = passed,
                Strengths = strengths,
                Gaps = gaps
            };
        }
    }
}
