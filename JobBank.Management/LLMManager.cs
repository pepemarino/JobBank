using JobBank.Management.Abstraction;
using JobBank.Models.Identity;
using JobBank.ModelsDTO;
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
        private const string DefaultVersion = "v1";

        public LLMManager(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public async Task<TargetModelDTO?> GetTargetModelAsync(string? userId = null)
        {            
            await using var scope = _scopeFactory.CreateAsyncScope();
            var prompService = scope.ServiceProvider.GetRequiredService<PrompService>();

            var user = await GetUserAsync(userId);

            if (user is not null && IsUserKey(user))
            {
                var systemKey = prompService.APIKeyEncryptionKayName;

                if (string.IsNullOrEmpty(systemKey))
                {
                    throw new InvalidOperationException("User CipherText exists but no master key is configured.");
                }

                var masterKey = ApiKeyEncryptor.GetMasterKey(systemKey);

                try
                {
                    return new TargetModelDTO
                    {
                        ApiKey = ApiKeyEncryptor.Decrypt(user.CipherText, user.Nonce, user.Tag, masterKey),
                        LLModel = user.LLModel,
                        UserId = user.Id,
                        Version = user.Version
                    };
                }
                catch (CryptographicException)
                {
                    return null;
                }
            }

            var apiKey = scope.ServiceProvider.GetRequiredService<ILLMProvider>().GetApiKey();

            return new TargetModelDTO
            {
                ApiKey = apiKey,
                LLModel = prompService.LLMModel,
                UserId = user?.Id ?? string.Empty,
                Version = DefaultVersion
            };
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
            var user = await GetUserAsync(userId);

            if (user is null)
            {
                return false;
            }

            return !string.IsNullOrEmpty(user.CipherText) &&
                   !string.IsNullOrEmpty(user.Nonce) &&
                   !string.IsNullOrEmpty(user.Tag) &&
                   !string.IsNullOrEmpty(user.LLModel);
        }

        private async Task<JobBankUser?> GetUserAsync(string? userId)
        {
            await using var scope = _scopeFactory.CreateAsyncScope();

            if (!string.IsNullOrEmpty(userId))
            {
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<JobBankUser>>();
                return await userManager.FindByIdAsync(userId);
            }

            var identityService = scope.ServiceProvider.GetRequiredService<IIdentityService>();
            return await identityService.GetCurrentUserDetailsAsync();
        }

        private static bool IsUserKey(JobBankUser? user)
        {
            return user is not null &&
                   !string.IsNullOrEmpty(user.CipherText) &&
                   !string.IsNullOrEmpty(user.Nonce) &&
                   !string.IsNullOrEmpty(user.Tag) &&
                   !string.IsNullOrEmpty(user.LLModel) &&
                   !string.IsNullOrEmpty(user.Version);
        }
    }
}