using DeliveryControlCenter.Infrastructure;
using DeliveryControlCenter.Worker;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();
builder.AddDeliveryInfrastructure();
builder.Services.AddHostedService<SyncWorker>();

var host = builder.Build();
await host.RunAsync();
