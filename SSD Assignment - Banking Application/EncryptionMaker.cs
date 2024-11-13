using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SSD_Assignment___Banking_Application
{
    public static class EncryptionMaker
    {
        private static readonly byte[] Key = Encoding.UTF8.GetBytes("ThisIsA16ByteKey");

        public static byte[] Encrypt(string plainText, CipherMode mode)
        {
            using (Aes aes = Aes.Create())
            {
                aes.KeySize = 128; // Explicitly set key size for AES-256
                aes.Key = Key;
                aes.Mode = mode;
                aes.Padding = PaddingMode.PKCS7;
                aes.GenerateIV();

                using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                using (var ms = new MemoryStream())
                {
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
                aes.KeySize = 128; // Explicitly set key size for AES-256
                aes.Key = Key;
                aes.Mode = mode;
                aes.Padding = PaddingMode.PKCS7;

                // Read the IV from the beginning of the cipherText
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
