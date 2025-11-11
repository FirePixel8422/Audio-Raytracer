using System;
using System.Security.Cryptography;
using System.Text;

public static class ObfuscationUtility
{
    private static readonly byte[] HmacKey = Encoding.UTF8.GetBytes("|9J%gNi(d8Nco#~l{"); // HMAC

    /// <summary>
    /// Obfuscate(Tag) the plain text with HMAC (make it unreadable)
    /// </summary>
    public static string Obfuscate(string plainText)
    {
        // Compute HMAC
        using HMACSHA256 hmac = new HMACSHA256(HmacKey);
        string tag = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(plainText)));

        return $"{tag}:{plainText}";
    }

    /// <summary>
    /// Restore(Remove) obfuscation from the tagged text
    /// </summary>
    public static (bool succes, string plainText) DeObfuscate(string taggedText)
    {
        string[] parts = taggedText.Split(new char[] { ':' }, 2);
        if (parts.Length != 2)
        {
            DebugLogger.LogError("File Loading DeObfuscation Failed: Invalid encrypted format");
            return (false, default);
        }

        string tag = parts[0];
        string plainText = parts[1];

        // Verify HMAC
        using HMACSHA256 hmac = new HMACSHA256(HmacKey);
        string computedTag = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(plainText)));

        if (!tag.Equals(computedTag))
        {
            DebugLogger.LogError("File Loading DeObfuscation Failed: Data integrity check failed");
            return (false, default);
        }

        return (true, plainText);
    }
}
