using JobBank.Management.Interview;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace JobBank.Util
{
    public static class Common
    {
        public static readonly JsonSerializerOptions StrictValidationOptions = new()
        {
            PropertyNameCaseInsensitive = false,
            ReadCommentHandling = JsonCommentHandling.Disallow,
            AllowTrailingCommas = false,
            UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow
        };

        public static Func<InterviewMetadata, bool> BusinessRulesFunc()
        {
            return i =>
            {
                var weakTopics =
                    i.Evaluations
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

                return weakTopics.Any();
            };
        }
    }
}
