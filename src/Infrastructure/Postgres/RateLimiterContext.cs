using System;
using SEA.DET.TarPit.Infrastructure.Postgres.Models;
using Microsoft.EntityFrameworkCore;

namespace SEA.DET.TarPit.Infrastructure.Postgres;

public class RateLimiterContext : DbContext
{
	public DbSet<Caller> Callers { get; set; }
	public DbSet<CallRecord> CallRecords { get; set; }

    public RateLimiterContext(DbContextOptions<RateLimiterContext> options)
        : base(options) { }

    public RateLimiterContext()
    {
    }

    protected override void OnConfiguring(
        DbContextOptionsBuilder dbContextOptionsBuilder)
    => dbContextOptionsBuilder
        // TODO: Factor conn string out into library instantiation
        .UseNpgsql(
    "Host=localhost;Database=tarpit;Username=tarpit;",
    o =>
    {
        o.UseNodaTime();
    })
        .UseSnakeCaseNamingConvention();


    protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<CallRecord>()
            .Property(p => p.CalledAt)
            .HasDefaultValueSql("NOW()");
	}
}

