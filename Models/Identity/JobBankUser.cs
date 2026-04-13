using Microsoft.AspNetCore.Identity;

namespace JobBank.Models.Identity
{
    public class JobBankUser : IdentityUser
    {
        /// <summary>
        /// This text is the encrypted version of the user's Model API key. 
        /// It is stored in the database as a Base64-encoded string.
        /// Decrypting this value requires the corresponding nonce and authentication tag, 
        /// as well as the master key used for encryption.
        /// </summary>
        public string? CipherText { get; set; }

        /// <summary>
        /// This is a unique random number used only once for the 
        /// encryption of the user's Model API key. It is stored in the database as a Base64-encoded string.
        /// </summary>
        public string? Nonce { get; set; }

        /// <summary>
        /// Gets or sets the tag associated with the object.
        /// This is an authentication tag generated during the encryption process of the user's 
        /// Model API key. It is stored in the database as a Base64-encoded string.
        /// </summary>
        public string? Tag { get; set; }

        /// <summary>
        /// Gets or sets the name or identifier of the large language model to use.
        /// This value is optional and can be used to specify which LLM the user prefers 
        /// when making requests that involve language model interactions.
        /// However, it is ignored if the user has not provided an API key, as the system 
        /// will fall back to using a system API key if available and its own model selection logic.
        /// </summary>
        public string? LLModel { get; set; }

        /// <summary>
        /// Gets or sets the flag indicating to never accept shared cache results for this user, 
        /// even if they are available.
        /// </summary>
        public bool ForceMyKeyy { get; set; } = false;
    }
}
