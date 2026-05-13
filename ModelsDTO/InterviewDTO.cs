namespace JobBank.ModelsDTO
{
    public class InterviewDTO : IEquatable<InterviewDTO>
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

        public bool IsDeleted { get; set; }

        public string JobTitle { get; set; } = string.Empty;

        public string Company { get; set; } = string.Empty;

        public int TrainingId { get; set; }

        public override bool Equals(object? obj)
        {
            return Equals(obj as InterviewDTO);
        }

        public bool Equals(InterviewDTO? other)
            => other is not null &&
               Id == other.Id &&
               JobPostId == other.JobPostId &&
               UserId == other.UserId &&
               Result == other.Result &&
               CreatedDateUtc == other.CreatedDateUtc &&
               StartedAtUtc == other.StartedAtUtc &&
               CompletedAtUtc == other.CompletedAtUtc &&
               Model == other.Model &&
               Prompt == other.Prompt &&
               ScoreTotal == other.ScoreTotal &&
               ScoreMax == other.ScoreMax &&
               Passed == other.Passed &&
               NumberOfQuestions == other.NumberOfQuestions &&
               IsCompleted == other.IsCompleted &&
               IsDeleted == other.IsDeleted &&
               JobTitle == other.JobTitle &&
               Company == other.Company &&
               TrainingId == other.TrainingId;

        public override int GetHashCode() => (
            Id,
            JobPostId,
            UserId,
            Result,
            CreatedDateUtc,
            StartedAtUtc,
            CompletedAtUtc,
            Model,
            Prompt,
            ScoreTotal,
            ScoreMax,
            Passed,
            NumberOfQuestions,
            IsCompleted,
            IsDeleted,
            JobTitle,
            Company,
            TrainingId).GetHashCode();
    }
}
