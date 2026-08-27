using DeliveryControlCenter.Infrastructure;
using DeliveryControlCenter.MigrationService;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();
builder.AddStoreInfrastructure();
builder.Services.AddHostedService<MigrationWorker>();

var host = builder.Build();
await host.RunAsync();
