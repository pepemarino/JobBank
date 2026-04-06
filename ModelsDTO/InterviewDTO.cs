namespace JobBank.ModelsDTO
{
    public class InterviewDTO
    {
        public int Id { get; set; }

        public int JobPostId { get; set; }

        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the result of the InterviewContent as a JSON string.
        /// </summary>
        public string Result { get; set; } = string.Empty;

        public DateTime CreatedDateUtc { get; set; }

        public DateTime StartedAtUtc { get; set; }
        public DateTime? CompletedAtUtc { get; set; }

        public string Model { get; set; } = string.Empty;
        public string Prompt { get; set; } = string.Empty;

        public decimal ScoreTotal { get; set; }
        public decimal ScoreMax { get; set; }
        public bool Passed { get; set; }

        public int NumberOfQuestions { get; set; }
        public bool IsCompleted { get; set; }
    }
}
