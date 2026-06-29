using JobBank.Models;

namespace JobBank.ModelsDTO
{
    public class EvaluationDTO
    {
        public int Id { get; set; }
        public int InterviewId { get; set; }
        public virtual Interview? Interview { get; set; }
        public string PreviousQuestion { get; set; } = string.Empty;
        public string PreviousTopic { get; set; } = string.Empty;
        public double Score { get; set; }
        public int Weight { get; set; }
        public bool Passed { get; set; }
        public string Strengths { get; set; } = string.Empty;
        public string Gaps { get; set; } = string.Empty;
        public string Evidence { get; set; } = string.Empty;
        public double Confidence { get; set; }
        public DateTime CreatedDateUtc { get; set; }
    }
}
