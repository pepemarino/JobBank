namespace JobBank.ModelsDTO
{
    public class TrainingDTO
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string Result { get; set; } = string.Empty;
        public DateTime CreatedDateUtc { get; set; }
        public string Model { get; set; } = string.Empty;
        public string Prompt { get; set; } = string.Empty;
        public int InterviewId { get; set; }
    }
}
