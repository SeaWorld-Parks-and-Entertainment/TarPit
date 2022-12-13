using System;
using Microsoft.EntityFrameworkCore;

namespace SEA.DET.TarPit.Infrastructure.Postgres.Models;

[Index(nameof(ExternalIdentifier), IsUnique = true)]
public class Caller
{
	public int Id { get; set; }
	public String ExternalIdentifier { get; set; }
    public int Difficulty { get; set; }

    public Caller(String externalIdentifier, int difficulty)
    {
        ExternalIdentifier = externalIdentifier;
        Difficulty = difficulty;
    }
}

