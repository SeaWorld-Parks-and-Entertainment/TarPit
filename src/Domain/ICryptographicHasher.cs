using System;
namespace SEA.DET.TarPit.Domain;

public interface ICryptographicHasher
{
    String Hash(String input);
}

