namespace JobBank.ModelsDTO
{
    public class UserSkillsDTO
    {
        public int? UserId { get; set; }

        public string RawSkills { get; set; } = string.Empty;

        public int Version { get; set; } = 1;

        /// <summary>
        /// Use this to know when the Analysis, if it exists is older than the updated
        /// If it is then we know it is of synch
        /// </summary>
        public DateTime? UpdatedAt { get; set; }
    }
}
