namespace JobBank.EtlConsole.Model
{
    public class EtlResultMetadata
    {
        public String MigrationName { get; set; } = string.Empty;
        public int ProcessedRecords { get; set; }
        public string Message { get; set; }
        public int TotalRecords { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
    }
}
