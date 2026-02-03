using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using JobBank.Models;

namespace JobBank.Data
{
    public class EmploymentBankContext : DbContext
    {
        public EmploymentBankContext (DbContextOptions<EmploymentBankContext> options)
            : base(options)
        {
        }

        public DbSet<JobBank.Models.JobPost> JobPost { get; set; } = default!;
        public DbSet<JobBank.Models.JobAnalysisCache> JobAnalysisCache { get; set; } = default!;
    }
}
