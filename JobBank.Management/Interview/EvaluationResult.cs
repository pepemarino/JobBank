namespace JobBank.Management.Interview
{
    /// <summary>
    /// The trainer agent will consume a list of these results to train the applicant (issue #23)
    /// </summary>
    public class EvaluationResult
    {
        public required string PreviousQuestion { get; set; } = string.Empty;
        public required string PreviousTopic { get; set; } = string.Empty;

        public required double Score { get; set; }           // 0 - 1
        public required int Weight { get; set; }             // 1 - 10
        public required bool Passed { get; set; }   
        
        public required List<string> Strengths { get; set; } = new();
        public required List<string> Gaps { get; set; } = new();
        
        public string Evidence { get; set; } = string.Empty;

        public required double Confidence { get; set; }

        public override int GetHashCode()
        {
            return HashCode
                .Combine(
                    PreviousQuestion, 
                    Score, 
                    Weight, 
                    Passed, 
                    PreviousTopic, 
                    Strengths, 
                    Gaps);
        }

        public override bool Equals(object? obj)
        {
            if (obj is not EvaluationResult other)
                return false;
            return PreviousQuestion == other.PreviousQuestion &&
                   Score == other.Score &&
                   Weight == other.Weight &&
                   Passed == other.Passed &&
                   PreviousTopic == other.PreviousTopic &&
                   Strengths.SequenceEqual(other.Strengths) &&
                   Gaps.SequenceEqual(other.Gaps);
        }
    }
}
