using System;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace SEA.DET.TarPit.Infrastructure.Postgres.Models;

[Index(nameof(CallerId), nameof(Nonce))]
[Index(nameof(CallerId), nameof(CalledAt))]
public class CallRecord
{
	public int Id { get; set; }
    public int CallerId { get; set; }
    public Instant CalledAt { get; set; }
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    // TODO: add a fixed length to this.
    public String Nonce { get; set; }
    public Caller Caller { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
}
