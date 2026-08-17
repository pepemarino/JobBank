using JobBank.Management.Interview;

namespace JobBank.Util
{
    public static class BusinessRules
    {
        /// <summary>
        /// This adds semantics to the codebase by encapsulating the logic for determining 
        /// if an interviewee needs training based on their interview evaluations. 
        /// It checks for weak topics based on average scores, failures, and meaningful gaps in knowledge. 
        /// By centralizing this logic in a single function, we can easily maintain and update 
        /// the criteria for training without scattering it throughout the codebase.
        /// If new business rules are required then create a new function to respect the Single Responsibility Principle 
        /// and keep the codebase clean and maintainable.
        /// </summary>
        /// <returns>
        /// A function delegate that takes an InterviewMetadata object and returns a boolean indicating whether the 
        /// interviewee needs training based on their evaluations.
        /// </returns>
        public static Func<InterviewMetadata, bool> InterviewTrainingRulesFunc()
        {
            return i =>
            {
                IEnumerable<IGrouping<string, EvaluationResult>> weakTopics = NeedsTrainingRule(i);
                return weakTopics.Any();
            };
        }

        public static IEnumerable<IGrouping<string, EvaluationResult>> NeedsTrainingRule(InterviewMetadata i)
        {
            return i.Evaluations
                    .Where(e => !string.IsNullOrWhiteSpace(e.PreviousTopic))
                    .GroupBy(e => e.PreviousTopic)
                    .Where(g =>
                    {
                        var avgScore = g.Average(e => e.Score);

                        var hasFailure =
                            g.Any(e => !e.Passed);

                        var hasMeaningfulGaps =
                            g.SelectMany(e => e.Gaps ?? [])
                             .Count(g =>
                                 !string.IsNullOrWhiteSpace(g) &&
                                 g.Length > 20) >= 2;

                        return
                            hasFailure ||
                            avgScore < 0.70 ||
                            (avgScore < 0.85 && hasMeaningfulGaps);
                    });
        }
    }
}
