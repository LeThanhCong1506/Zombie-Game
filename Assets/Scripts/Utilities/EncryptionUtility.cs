using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

public static class EncryptionUtility
{
    private static readonly string encryptionKey = "aae1c7680a136348832a2e5e2bb8fc4c8045f71ef2fdd4695f540676bac6088f";

    public static string EncryptString(string plainText)
    {
        byte[] key = Encoding.UTF8.GetBytes(encryptionKey.Substring(0, 32)); // Ensure key is 32 bytes long for AES-256
        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = key;
            aesAlg.GenerateIV();
            ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

            using (var msEncrypt = new MemoryStream())
            {
                msEncrypt.Write(aesAlg.IV, 0, aesAlg.IV.Length); // Write IV to the beginning of the stream
                using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                using (var swEncrypt = new StreamWriter(csEncrypt))
                {
                    swEncrypt.Write(plainText);
                }
                return Convert.ToBase64String(msEncrypt.ToArray()); // Return encrypted data as Base64 string
            }
        }
    }

    public static string DecryptString(string cipherText)
    {
        byte[] fullCipher = Convert.FromBase64String(cipherText);
        byte[] iv = new byte[16];
        byte[] cipher = new byte[fullCipher.Length - 16];

        Array.Copy(fullCipher, iv, iv.Length);
        Array.Copy(fullCipher, 16, cipher, 0, cipher.Length);

        byte[] key = Encoding.UTF8.GetBytes(encryptionKey.Substring(0, 32));
        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = key;
            aesAlg.IV = iv;
            ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

            using (var msDecrypt = new MemoryStream(cipher))
            using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
            using (var srDecrypt = new StreamReader(csDecrypt))
            {
                return srDecrypt.ReadToEnd(); // Return the decrypted text
            }
        }
    }
}

