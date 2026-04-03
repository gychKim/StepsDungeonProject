public static class AES
{
    public static byte[] AESEncrypt(string plainText, byte[] key, byte[] iv)
    {
        using (System.Security.Cryptography.Aes aesAlg = System.Security.Cryptography.Aes.Create())
        {
            aesAlg.Key = key;
            aesAlg.IV = iv;
            aesAlg.Mode = System.Security.Cryptography.CipherMode.CBC;
            aesAlg.Padding = System.Security.Cryptography.PaddingMode.PKCS7;

            System.Security.Cryptography.ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

            using (System.IO.MemoryStream msEncrypt = new System.IO.MemoryStream())
            using (System.Security.Cryptography.CryptoStream csEncrypt = new System.Security.Cryptography.CryptoStream(msEncrypt, encryptor, System.Security.Cryptography.CryptoStreamMode.Write))
            using (System.IO.StreamWriter swEncrypt = new System.IO.StreamWriter(csEncrypt))
            {
                swEncrypt.Write(plainText);
                swEncrypt.Flush();
                csEncrypt.FlushFinalBlock();
                return msEncrypt.ToArray();
            }
        }
    }

    public static string AESDecrypt(byte[] cipherBytes, byte[] key, byte[] iv)
    {
        using (System.Security.Cryptography.Aes aesAlg = System.Security.Cryptography.Aes.Create())
        {
            aesAlg.Key = key;
            aesAlg.IV = iv;
            aesAlg.Mode = System.Security.Cryptography.CipherMode.CBC;
            aesAlg.Padding = System.Security.Cryptography.PaddingMode.PKCS7;

            System.Security.Cryptography.ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

            using (System.IO.MemoryStream msDecrypt = new System.IO.MemoryStream(cipherBytes))
            using (System.Security.Cryptography.CryptoStream csDecrypt = new System.Security.Cryptography.CryptoStream(msDecrypt, decryptor, System.Security.Cryptography.CryptoStreamMode.Read))
            using (System.IO.StreamReader srDecrypt = new System.IO.StreamReader(csDecrypt))
            {
                return srDecrypt.ReadToEnd();
            }
        }
    }
}
