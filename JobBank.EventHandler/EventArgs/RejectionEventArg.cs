namespace JobBank.EventHandler.EventArgs
{
    public class RejectionEventArg
    {
        public int JobPostId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string TerminationReason { get; set; } = string.Empty;

        public RejectionEventArg() { }

        public RejectionEventArg(int jobId, string userId, string terminationReason)
        {
            JobPostId = jobId;
            UserId = userId;
            TerminationReason = terminationReason;
        }
    }
}
