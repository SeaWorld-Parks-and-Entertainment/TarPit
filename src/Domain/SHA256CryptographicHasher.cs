using System;
using System.Security.Cryptography;
using System.Text;

namespace SEA.DET.TarPit.Domain;

public class SHA256CryptographicHasher : ICryptographicHasher
{

    public SHA256CryptographicHasher()
	{
	}

    public String Hash(String input)
    {
        byte[] inputBytes = Encoding.Default.GetBytes(input);
        byte[] hashBytes = SHA256.HashData(inputBytes);
        return BitConverter.ToString(hashBytes)
            .Replace("-", "")
            .ToLowerInvariant();
    }
}

