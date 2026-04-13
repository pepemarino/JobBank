using JobBank.Management.Abstraction;
using JobBank.Models.Identity;
using JobBank.Services;
using JobBank.Services.Abstraction;
using JobBank.StartUpServices;
using Microsoft.AspNetCore.Identity;
using System.Security.Cryptography;

namespace JobBank.Management
{
    public class LLMManager : ILLMManager
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public LLMManager(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public async Task<string?> GetApiKeyAsync(string? userId = null)
        {
            await using var scope = _scopeFactory.CreateAsyncScope();

            JobBankUser? user = null;

            // If userId provided (e.g., from background service), fetch user directly
            if (!string.IsNullOrEmpty(userId))
            {
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<JobBankUser>>();
                user = await userManager.FindByIdAsync(userId);
            }
            else
            {
                // Fall back to getting current user from authentication (Razor component context)
                var identityService = scope.ServiceProvider.GetRequiredService<IIdentityService>();
                user = await identityService.GetCurrentUserDetailsAsync();
            }

            if (user is not null &&
                !string.IsNullOrEmpty(user.CipherText) &&
                !string.IsNullOrEmpty(user.Nonce) &&
                !string.IsNullOrEmpty(user.Tag) &&
                !string.IsNullOrEmpty(user.LLModel))
            {
                var prompService = scope.ServiceProvider.GetRequiredService<PrompService>();
                var systemKey = prompService.APIKeyEncryptionKayName;

                if (string.IsNullOrEmpty(systemKey))
                {
                    throw new InvalidOperationException("User CipherText exists but no master key is configured.");
                }

                var masterKey = ApiKeyEncryptor.GetMasterKey(systemKey);

                try
                {
                    return ApiKeyEncryptor.Decrypt(user.CipherText, user.Nonce, user.Tag, masterKey);
                }
                catch (CryptographicException)
                {
                    return null;
                }
            }

            return scope.ServiceProvider.GetRequiredService<ILLMProvider>().GetApiKey();
        }

        public async Task<bool> IsAvailableAsync(string? userId = null)
        {
            var isPrivateKeyAvailable = await UserHasValidPrivateKeyAsync(userId);
            if (isPrivateKeyAvailable)
            {
                return true;
            }
            await using var scope = _scopeFactory.CreateAsyncScope();
            var llmProvider = scope.ServiceProvider.GetRequiredService<ILLMProvider>();
            return llmProvider.IsAvailable;
        }

        public async Task<bool> UserHasValidPrivateKeyAsync(string? userId)
        {
            await using var scope = _scopeFactory.CreateAsyncScope();

            JobBankUser? user = null;

            // If userId provided (e.g., from background service), fetch user directly
            if (!string.IsNullOrEmpty(userId))
            {
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<JobBankUser>>();
                user = await userManager.FindByIdAsync(userId);
            }
            else
            {
                // Fall back to getting current user from authentication (Razor component context)
                var identityService = scope.ServiceProvider.GetRequiredService<IIdentityService>();
                user = await identityService.GetCurrentUserDetailsAsync();
            }

            if (user is null)
            {
                return false;
            }

            if (!string.IsNullOrEmpty(user.CipherText) &&
                !string.IsNullOrEmpty(user.Nonce) &&
                !string.IsNullOrEmpty(user.Tag) &&
                !string.IsNullOrEmpty(user.LLModel))
            {
                return true;
            }

            return false;
        }
    }
}