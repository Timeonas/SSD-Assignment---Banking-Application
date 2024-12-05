using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace SSD_Assignment___Banking_Application
{
    public static class EncryptionMaker
    {
        private const string KeyFilePath = "encryption_key.dat"; //Path to store the encrypted key

        // Generate, protect, and store the key (DPAPI)
        private static byte[] GetOrCreateKey()
        {
            if (!File.Exists(KeyFilePath))
            {
                // Generate a random 16-byte key for AES-128
                byte[] key = new byte[16];
                using (var rng = new RNGCryptoServiceProvider())
                {
                    rng.GetBytes(key);
                }

                // Protect the key using DPAPI
                byte[] protectedKey = ProtectedData.Protect(key, null, DataProtectionScope.CurrentUser);

                // Save the encrypted key to a file
                File.WriteAllBytes(KeyFilePath, protectedKey);
                return key;
            }
            else
            {
                // Read the encrypted key from the file
                byte[] protectedKey = File.ReadAllBytes(KeyFilePath);

                // Unprotect the key using DPAPI
                return ProtectedData.Unprotect(protectedKey, null, DataProtectionScope.CurrentUser);
            }
        }

        private static readonly byte[] Key = GetOrCreateKey();

        public static byte[] Encrypt(string plainText, CipherMode mode)
        {
            using (Aes aes = Aes.Create())
            {
                aes.KeySize = 128; // For AES-128
                aes.Key = Key;
                aes.Mode = mode;
                aes.Padding = PaddingMode.PKCS7;
                aes.GenerateIV();

                using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                using (var ms = new MemoryStream())
                {
                    // Write the IV to the beginning of the stream
                    ms.Write(aes.IV, 0, aes.IV.Length);

                    using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    using (var writer = new StreamWriter(cs))
                    {
                        writer.Write(plainText);
                    }

                    return ms.ToArray();
                }
            }
        }

        public static string Decrypt(byte[] cipherText, CipherMode mode)
        {
            using (Aes aes = Aes.Create())
            {
                aes.KeySize = 128; //For AES-128
                aes.Key = Key;
                aes.Mode = mode;
                aes.Padding = PaddingMode.PKCS7;

                //Read the IV from the beginning of the cipherText
                byte[] iv = new byte[aes.BlockSize / 8];
                Array.Copy(cipherText, iv, iv.Length);
                aes.IV = iv;

                using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                using (var ms = new MemoryStream(cipherText, iv.Length, cipherText.Length - iv.Length))
                using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                using (var reader = new StreamReader(cs))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}
