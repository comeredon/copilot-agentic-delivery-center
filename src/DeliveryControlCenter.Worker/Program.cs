using DeliveryControlCenter.Infrastructure;
using DeliveryControlCenter.Worker;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();
builder.AddStoreInfrastructure();
builder.Services.AddHostedService<CoffeeOperationsWorker>();

var host = builder.Build();
await host.RunAsync();
