using JobBank.Services.Abstraction;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace JobBank.Services
{
    public class IdentityService : IIdentityService
    {
        private readonly AuthenticationStateProvider _authStateProvider;
        public IdentityService(AuthenticationStateProvider authStateProvider)
        {
            _authStateProvider = authStateProvider;
        }

        public async Task<ClaimsPrincipal> GetCurrentUserAsync()
        {
            var authState = await _authStateProvider.GetAuthenticationStateAsync();
            return authState.User;
        }

        public async Task<bool> IsInRoleAsync(string role)
        {
            var state = await _authStateProvider.GetAuthenticationStateAsync();
            return state.User.IsInRole(role);
        }

        public async Task<string?> GetUserIdAsync()
        {
            var authState = await _authStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;

            return user.Identity is { IsAuthenticated: true }
                ? user.FindFirst(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                : null;
        }

        public async Task<string> GetCurrentUserNameAsync()
        {
            var authState = await _authStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;

            return user != null && user.Identity!.IsAuthenticated
                ? user.Identity!.Name!
                : "Guest";
        }
    }
}
