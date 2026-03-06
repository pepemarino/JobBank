using JobBank.Models.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AuthSample.Data;

public class JobBankIdentityDbContext(DbContextOptions<JobBankIdentityDbContext> options) : IdentityDbContext<JobBankUser>(options)
{
}