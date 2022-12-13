using SEA.DET.TarPit.Infrastructure.Postgres;
using SEA.DET.TarPit.Library;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;

var builder = WebApplication.CreateBuilder(args);
builder.Services
    .AddProofOfWorkRateLimiting<ProofOfWorkRateLimiterOptions>()
    .AddDbContext<RateLimiterContext>(options =>
    {
        options.UseNpgsql("host=localhost;database=tarpit;user=tarpit");
    }
    )
    .AddProofOfWorkRateLimiting<ProofOfWorkRateLimiterOptions>()
    .AddMvc((options) => options.EnableEndpointRouting = false);

var app = builder.Build();
app.UseProofOfWorkRateLimitingMiddleware();
app.UseRouting();
// Disregard this warning, .NET Core is being snotty about that #nodeps lifestyle.
app.UseEndpoints(endpoints =>
{
    endpoints.MapGet("/ok", async context =>
    {
        context.Response.StatusCode = 200;
        await context.Response.WriteAsync("ok");
    });
    endpoints.MapGet("/notok", async context =>
    {
        context.Response.StatusCode = 200;
        await context.Response.WriteAsync("notok");
    });
});
app.UseDeveloperExceptionPage();
app.Run();
