using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SSD_Assignment___Banking_Application
{
    public static class EncriptionTest
    {
        public static void RunEncryptionTests()
        {
            // Sample PII data to test
            string originalText = "Sensitive PII Data";

            try
            {
                // Test encryption in CFB mode
                byte[] encryptedData = EncryptionMaker.Encrypt(originalText, CipherMode.CFB);
                Console.WriteLine("Encrypted (Base64): " + Convert.ToBase64String(encryptedData));

                // Test decryption
                string decryptedText = EncryptionMaker.Decrypt(encryptedData, CipherMode.CFB);
                Console.WriteLine("Decrypted: " + decryptedText);

                // Verify that decrypted text matches the original
                if (originalText == decryptedText)
                {
                    Console.WriteLine("Test Passed: Decrypted text matches the original.");
                }
                else
                {
                    Console.WriteLine("Test Failed: Decrypted text does not match the original.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred during encryption testing: " + ex.Message);
            }
        }
    }
}
