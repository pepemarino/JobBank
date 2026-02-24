namespace JobBank.ModelsDTO
{
    public class RejectionAnalysisDTO
    {
        public int Id { get; set; }
        public int JobApplicationId { get; set; }
        public string? RejectionReason { get; set; }
        public DateTime AnalysisDate { get; set; }
    }
}
