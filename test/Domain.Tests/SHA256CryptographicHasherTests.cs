using System;
using Xunit;

namespace SEA.DET.TarPit.Domain.Tests;

public class SHA256CryptographicHasherTests
{
	SHA256CryptographicHasher sha256CryptographicHasher =
		new SHA256CryptographicHasher();
	public SHA256CryptographicHasherTests()
	{
	}

	[Fact]
	public void HashingWorksWithNoncesGeneratedInBash()
	{
		String nonce = "1ca945e46f9a7985217845f450114a9a";
        /// printf '1ca945e46f9a7985217845f450114a9a' | sha256sum
        String expectedHash =
            "0e524ff087885d07e04f5ac3ab8ba7baabe35db3cc3f688dfb082cf9fb8c8192";

		String actualHash = sha256CryptographicHasher.Hash(nonce);

		Assert.Equal(expectedHash, actualHash);
    }
}

