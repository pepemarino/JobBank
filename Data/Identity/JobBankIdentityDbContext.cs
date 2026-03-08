using JobBank.Models.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace JobBank.Data;

public class JobBankIdentityDbContext
    : IdentityDbContext<JobBankUser, IdentityRole, string>
{
    public JobBankIdentityDbContext(
        DbContextOptions<JobBankIdentityDbContext> options)
        : base(options)
    {
    }
}