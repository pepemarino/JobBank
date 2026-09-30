using JobBank.Extensions;
using JobBank.Models;
using System.ComponentModel.DataAnnotations;

namespace JobBank.Components.Pages.JobPostPages.ViewModels
{
    public class JobPostDataModel
    {
        public JobPostDataModel()
        {
                
        }

        public JobPostDataModel(JobPost post)
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
            AutomaticallyRejected = post.AutomaticallyRejected;
            IsSelfWitdrawn = post.IsSelfWitdrawn;
        }

        public int Id { get; set; }

        [Required]
        public string? Title { get; set; }

        public string? TitleDisplay => string.IsNullOrEmpty(Title) ? string.Empty : Title.TruncateByLength();

        public bool IsApplied { get; set; }

        //#ffe5d0
        public bool IsSelfWitdrawn { get; set; }

        [Required]
        public string? Company { get; set; }

        public string? CompanyDisplay => string.IsNullOrEmpty(Company) ? string.Empty : Company.TruncateByLength(25);

        public string? ActionToTake { get; set; }

        [Required]
        public string? JobType { get; set; }

        [Required]
        public DateTime? ApplicationDate { get; set; }

        public DateTime? InterviewDate { get; set; }

        public string? InterviewOutcome { get; set; }

        public string? InterviewOutcomeDisplay => string.IsNullOrEmpty(InterviewOutcome) ? string.Empty : InterviewOutcome.TruncateByLength();

        public bool ApplicationDeclined { get; set; }

        public bool AutomaticallyRejected { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="jobPost"></param>
        public static implicit operator JobPostDataModel(JobPost jobPost)
        {
            return new JobPostDataModel
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
                ApplicationDeclined = jobPost.ApplicationDeclined,
                AutomaticallyRejected = jobPost.AutomaticallyRejected,  
                IsSelfWitdrawn = jobPost.IsSelfWitdrawn
            };
        }
    }
}
