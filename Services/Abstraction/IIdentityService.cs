using System.Security.Claims;

namespace JobBank.Services.Abstraction
{
    public interface IIdentityService
    {
        Task<ClaimsPrincipal> GetCurrentUserAsync();
        Task<bool> IsInRoleAsync(string role);
        Task<string?> GetUserIdAsync();
        Task<string> GetCurrentUserNameAsync(); 
    }
}
