using System.Security.Cryptography;

namespace PangyaAPI.Utilities.Cryptography;

public static class Sha256
{
    public static string ComputeFileHex(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        using FileStream input = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(input));
    }
}
