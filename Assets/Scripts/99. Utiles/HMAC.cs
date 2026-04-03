using System.Linq;
using System.Security.Cryptography;

public static class HMAC
{
    public static byte[] ComputeHMAC(byte[] data, byte[] key)
    {
        using (var hmac = new HMACSHA256(key))
        {
            return hmac.ComputeHash(data);
        }
    }

    public static bool VerifyHMAC(byte[] data, byte[] key, byte[] expectedHmac)
    {
        var computed = ComputeHMAC(data, key);
        return CryptographicOperations.FixedTimeEquals(computed, expectedHmac);
    }
}