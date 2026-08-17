using JobBank.Models.Identity;
using JobBank.Services.Services.Abstraction;
using Microsoft.AspNetCore.Identity;

namespace JobBank.Services.Services
{
    public class UserRepository : IUserRepository
    {
        private readonly UserManager<JobBankUser> _userManager;

        public UserRepository(UserManager<JobBankUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<JobBankUser?> GetUserByIdAsync(string userId)
        {
            return await _userManager.FindByIdAsync(userId);
        }
    }
}
