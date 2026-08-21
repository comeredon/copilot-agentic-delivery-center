using DeliveryControlCenter.Api.Endpoints;
using DeliveryControlCenter.Api.Realtime;
using DeliveryControlCenter.Application.Abstractions;
using DeliveryControlCenter.Contracts.Realtime;
using DeliveryControlCenter.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddDeliveryInfrastructure();

// The API owns the realtime transport, so it replaces the no-op notifier.
builder.Services.AddSingleton<IDeliveryNotifier, SignalRDeliveryNotifier>();
builder.Services.AddSignalR();
builder.Services.AddProblemDetails();
builder.Services.AddDeliveryRateLimiting();
builder.Services.AddOpenApi();

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseRateLimiter();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
else
{
    app.UseHsts();
}

app.UseHttpsRedirection();

app.MapDefaultEndpoints();
app.MapDeliveryEndpoints();
app.MapHub<DeliveryHub>(DeliveryHubContract.Path);

await app.RunAsync();

/// <summary>Exposed so integration tests can boot the real pipeline with WebApplicationFactory.</summary>
public sealed partial class Program;
