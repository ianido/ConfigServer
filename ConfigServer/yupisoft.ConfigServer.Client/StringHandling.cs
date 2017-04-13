using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;


namespace yupisoft.ConfigServer.Client
{
    public static class StringHandling
    {
        public static string DefaultSalt = "o6806642kbM7c5";
        public static UInt64 CalculateHash(string read)
        {
            UInt64 hashedValue = 3074457345618258791ul;
            for (int i = 0; i < read.Length; i++)
            {
                hashedValue += read[i];
                hashedValue *= 3074457345618258799ul;
            }
            return hashedValue;
        }

        public static string Encrypt(string encryptString, string encryptionKey, string salt)
        {
            byte[] clearBytes = Encoding.Unicode.GetBytes(encryptString);
            return Convert.ToBase64String(Encrypt(clearBytes, encryptionKey, salt));            
        }

        public static byte[] Encrypt(byte[] clearBytes, string encryptionKey, string salt)
        {
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(encryptionKey, Encoding.ASCII.GetBytes(salt));
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(clearBytes, 0, clearBytes.Length);
                    }
                    clearBytes = ms.ToArray();
                }
            }
            return clearBytes;
        }

        public static string Decrypt(string cipherText, string encryptionKey, string salt)
        {
            cipherText = cipherText.Replace(" ", "+");
            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            return Encoding.Unicode.GetString(Decrypt(cipherBytes, encryptionKey, salt));
        }

        public static byte[] Decrypt(byte[] cipherBytes, string encryptionKey, string salt)
        {
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(encryptionKey, Encoding.ASCII.GetBytes(salt));
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(cipherBytes, 0, cipherBytes.Length);
                    }
                    cipherBytes = ms.ToArray();
                }
            }
            return cipherBytes;
        }

    }
}
