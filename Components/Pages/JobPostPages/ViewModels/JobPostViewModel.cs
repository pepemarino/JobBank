using JobBank.Models;
using System.ComponentModel.DataAnnotations;

namespace JobBank.Components.Pages.JobPostPages.ViewModels
{
    public class JobPostViewModel
    {
        public JobPostViewModel()
        {
                
        }

        public JobPostViewModel(JobPost post)
        {
            Id = post.Id;
            Title = post.Title;
            Company = post.Company;
            InterviewDate = post.InterviewDate;
            InterviewOutcome = post.InterviewOutcome;
            IsApplied = post.IsApplied;
            JobType = post.JobType;
            ActionToTake = post.ActionToTake;
            ApplicationDate = post.ApplicationDate;
            ApplicationDeclined = post.ApplicationDeclined;
        }

        public int Id { get; set; }

        [Required]
        public string? Title { get; set; }

        public bool IsApplied { get; set; }

        [Required]
        public string? Company { get; set; }

        public string? ActionToTake { get; set; }

        [Required]
        public string? JobType { get; set; }

        [Required]
        public DateTime? ApplicationDate { get; set; }

        public DateTime? InterviewDate { get; set; }

        public string? InterviewOutcome { get; set; }

        public bool ApplicationDeclined { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="jobPost"></param>
        public static implicit operator JobPostViewModel(JobPost jobPost)
        {
            return new JobPostViewModel
            {
                Id = jobPost.Id,
                Title = jobPost.Title,
                Company = jobPost.Company,  
                InterviewDate = jobPost.InterviewDate,
                InterviewOutcome = jobPost.InterviewOutcome,    
                IsApplied = jobPost.IsApplied,    
                JobType = jobPost.JobType,  
                ActionToTake = jobPost.ActionToTake,
                ApplicationDate = jobPost.ApplicationDate,
                ApplicationDeclined = jobPost.ApplicationDeclined
            };
        }
    }
}
