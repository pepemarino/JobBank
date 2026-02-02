using System.ComponentModel.DataAnnotations;

namespace JobBank.Models
{
    public class StudyGuide
    {
        public int Id { get; set; }
        public int JobPostId { get; set; }

        [Required]
        public string? JsonInterviewQuestion { get; set; }

        [Required]
        public string? JsonStudyContent { get; set; }     
    }
}
