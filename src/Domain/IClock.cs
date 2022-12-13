using System;
namespace SEA.DET.TarPit.Domain;

public interface IClock
{
    DateTimeOffset Now { get; }
}

