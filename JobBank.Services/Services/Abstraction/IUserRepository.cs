using JobBank.Models.Identity;

namespace JobBank.Services.Services.Abstraction
{
    public interface IUserRepository
    {
        Task<JobBankUser?> GetUserByIdAsync(string userId);
    }
}
