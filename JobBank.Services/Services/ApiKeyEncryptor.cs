using System.Security.Cryptography;
using System.Text;

namespace JobBank.Services
{
    public static class ApiKeyEncryptor
    {
        /// <summary>
        /// Encrypts the specified plaintext using AES-GCM and returns the ciphertext, nonce, and authentication tag as
        /// Base64-encoded strings.
        /// </summary>
        /// <remarks>The method uses AES-GCM for authenticated encryption. The returned values are
        /// Base64-encoded to facilitate storage and transmission. The same key, nonce, and tag are required to decrypt
        /// the ciphertext. This method generates a random nonce for each encryption operation.</remarks>
        /// <param name="plaintext">The plaintext string to be encrypted. Cannot be null.</param>
        /// <param name="key">The encryption key as a byte array. Must be 16, 24, or 32 bytes in length to match 
        /// AES key sizes. This key is the master key used for encryption and decryption. It should be securely stored and protected, 
        /// as anyone with access to
        /// </param>
        /// <returns>A tuple containing the Base64-encoded ciphertext, nonce, and authentication tag. The caller must retain the
        /// nonce and tag for decryption.</returns>
        public static (string CipherText, string Nonce, string Tag) Encrypt(string plaintext, byte[] key)
        {
            byte[] nonce = RandomNumberGenerator.GetBytes(12);
            byte[] plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
            byte[] cipherBytes = new byte[plaintextBytes.Length];
            byte[] tag = new byte[16];

            using (var aes = new AesGcm(key))
            {
                aes.Encrypt(nonce, plaintextBytes, cipherBytes, tag);
            }

            return (
                Convert.ToBase64String(cipherBytes),
                Convert.ToBase64String(nonce),
                Convert.ToBase64String(tag)
            );
        }

        /// <summary>
        /// How to use this method:
        /// 
        ///  if (!string.IsNullOrEmpty(user.CipherText))
        ///  {
        ///     var apiKey = ApiKeyEncryptor
        ///        .Decrypt(
        ///            cipherTextB64: user.CipherText, 
        ///            nonceB64: user.Nonce!, 
        ///            tagB64: user.Tag!, 
        ///            ApiKeyEncryptor.GetMasterKey(PrompService.APIKeyEncryptionKayName));
        ///  }
        /// </summary>
        /// <param name="cipherTextB64"></param>
        /// <param name="nonceB64"></param>
        /// <param name="tagB64"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string Decrypt(string cipherTextB64, string nonceB64, string tagB64, byte[] key)
        {
            byte[] nonce = Convert.FromBase64String(nonceB64);
            byte[] cipherBytes = Convert.FromBase64String(cipherTextB64);
            byte[] tag = Convert.FromBase64String(tagB64);

            byte[] plaintextBytes = new byte[cipherBytes.Length];

            using (var aes = new AesGcm(key))
            {
                aes.Decrypt(nonce, cipherBytes, tag, plaintextBytes);
            }

            return Encoding.UTF8.GetString(plaintextBytes);
        }

        //// Load master key from environment variable or create a new one if it doesn't exist. The key is stored as a Base64 string in the environment variable.
        //// and it should happen only once.
        //public static byte[] GetMasterKey(string apiKeyEncryptionKeyNake)
        //{
        //    string base64Key = Environment.GetEnvironmentVariable(apiKeyEncryptionKeyNake);
        //    if (base64Key == null)
        //    {
        //        byte[] newKey = new byte[32]; // 256-bit
        //        using (var rng = RandomNumberGenerator.Create())
        //            rng.GetBytes(newKey);

        //        Environment.SetEnvironmentVariable(
        //                    apiKeyEncryptionKeyNake,
        //                    Convert.ToBase64String(newKey),
        //                    EnvironmentVariableTarget.Machine       // store at machine level so it's available to all users and processes on the machine.
        //                                                            // This is important because the key needs to be accessible across different sessions
        //                                                            // and by the application regardless of which user is running IIS.
        //                );

        //        return newKey;
        //    }

        //    byte[] key = Convert.FromBase64String(base64Key);

        //    if (key.Length != 32)
        //        throw new InvalidOperationException("Encryption key must be 32 bytes (256 bits) in length.");

        //    return key;
        //}

        public static byte[] GetMasterKey(string apiKeyEncryptionKeyNake)
        {
            try
            {
                if (File.Exists(apiKeyEncryptionKeyNake))
                {
                    byte[] encryptedRead = File.ReadAllBytes(apiKeyEncryptionKeyNake);
                    byte[] key = ProtectedData.Unprotect(
                        encryptedRead,
                        null,
                        DataProtectionScope.LocalMachine
                    );

                    if (key.Length != 32)
                        throw new InvalidOperationException("Encryption key must be 32 bytes (256 bits) in length.");
                    return key;
                }

                byte[] newKey = new byte[32];
                using (var rng = RandomNumberGenerator.Create())
                    rng.GetBytes(newKey);

                byte[] encryptedWrite = ProtectedData.Protect(
                    newKey,
                    null,
                    DataProtectionScope.LocalMachine
                );

                File.WriteAllBytes(apiKeyEncryptionKeyNake, encryptedWrite);
                return newKey;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to load or decrypt the master key from {apiKeyEncryptionKeyNake}.", ex);
            }
        }
    }
}
