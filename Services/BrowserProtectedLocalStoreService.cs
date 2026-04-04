using JobBank.Services.Abstraction;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace JobBank.Services
{
    public class BrowserProtectedLocalStoreService<T> : IProtectedLocalStoreService<T> where T : class
    {
        private readonly ProtectedLocalStorage _storage;

        public BrowserProtectedLocalStoreService(ProtectedLocalStorage storage)
        {
            _storage = storage;
        }

        /// <summary>
        /// This method retrieves the interview state from the browser's local storage using a key that includes the job post ID.
        /// If a user loads the same interview in a different tab or window, they will see 
        /// the most recently saved state, allowing for a seamless experience across multiple sessions.
        /// </summary>
        /// <param name="jobPostId"></param>
        /// <returns></returns>
        public async Task<T?> LoadAsync(string key)
        {
            //var key = $"interview-state-{jobPostId}";
            var result = await _storage.GetAsync<T>(key);
            return result.Success ? result.Value : null;
        }

        /// <summary>
        /// This method saves the interview state to the browser's local storage using a 
        /// key that includes the job post ID. This allows the application to persist
        /// the interview state across page reloads or browser sessions, ensuring that 
        /// users can continue their interview process without losing progress.
        /// </summary>
        /// <param name="jobPostId"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public async Task SaveAsync(string key, T state)
        {
            //var key = $"interview-state-{jobPostId}";
            await _storage.SetAsync(key, state);
        }

        public async Task ClearAsync(string key)
        {
            //var key = $"interview-state-{jobPostId}";
            await _storage.DeleteAsync(key);
        }
    }

}
