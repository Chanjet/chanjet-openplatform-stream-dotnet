using System;
using System.Security.Cryptography;
using System.Text;

namespace Chanjet.Connector.Sdk
{
    public static class CryptoUtils
    {
        public static string DecryptAES(string cipherText, string key)
        {
            if (string.IsNullOrEmpty(cipherText)) return cipherText;

            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            if (keyBytes.Length > 16)
            {
                byte[] temp = new byte[16];
                Array.Copy(keyBytes, temp, 16);
                keyBytes = temp;
            }
            else if (keyBytes.Length < 16)
            {
                byte[] temp = new byte[16];
                Array.Copy(keyBytes, temp, keyBytes.Length);
                keyBytes = temp;
            }

            byte[] cipherBytes = Convert.FromBase64String(cipherText);

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = keyBytes;
                aesAlg.Mode = CipherMode.ECB;
                aesAlg.Padding = PaddingMode.PKCS7;

                using (ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV))
                {
                    byte[] plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
                    return Encoding.UTF8.GetString(plainBytes);
                }
            }
        }
    }
}
