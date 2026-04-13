namespace JobBank.Management.Abstraction
{
    public interface ILLMManager
    {
        /// <summary>
        /// This method chesks if an API Key is available either private of puclic.  
        /// If userId is provided, it checks for a private key for that user.  
        /// If not, it checks for a public key.
        /// It checks only if intergration with the LLM is possible.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<bool> IsAvailableAsync(string? userId = null);

        /// <summary>
        /// Checks is the user has a privage key. Dotaing a computed result 
        /// depends on having a private key
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<bool> UserHasValidPrivateKeyAsync(string userId);

        Task<string?> GetApiKeyAsync(string? userId = null);
    }
}
