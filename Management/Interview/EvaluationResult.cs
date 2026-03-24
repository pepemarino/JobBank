namespace JobBank.Management.Interview
{
    /// <summary>
    /// this class is still in design.  It is meant to capture the results of the interview evaluation, 
    /// including the score, whether the candidate passed, and lists of strengths and gaps identified during the evaluation.
    /// The trainer agent will consume a list of these results to train the applicant (issue #23)
    /// </summary>
    public class EvaluationResult
    {
        public double Score { get; set; }
        public bool Passed { get; set; }    
        public List<string> Strengths { get; set; } = new();
        public List<string> Gaps { get; set; } = new();
    }
}
