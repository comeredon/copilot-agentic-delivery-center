using DeliveryControlCenter.Api.Endpoints;
using DeliveryControlCenter.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddStoreInfrastructure();

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
    app.UseHttpsRedirection();
}

app.MapDefaultEndpoints();
app.MapCatalogEndpoints();

await app.RunAsync();

/// <summary>Exposed so integration tests can boot the real pipeline with WebApplicationFactory.</summary>
public sealed partial class Program;
