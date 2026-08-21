var builder = DistributedApplication.CreateBuilder(args);

// Postgres keeps a data volume so a restart does not wipe the delivery history.
var postgres = builder.AddPostgres("postgres")
    .WithDataVolume("dcc-postgres-data")
    .WithPgAdmin();

var deliveryDb = postgres.AddDatabase("deliverydb");

var cache = builder.AddRedis("cache")
    .WithRedisInsight();

// Migrations run to completion before anything else touches the schema.
var migrations = builder.AddProject<Projects.DeliveryControlCenter_MigrationService>("migrations")
    .WithReference(deliveryDb)
    .WaitFor(deliveryDb);

var api = builder.AddProject<Projects.DeliveryControlCenter_Api>("api")
    .WithReference(deliveryDb)
    .WithReference(cache)
    .WaitFor(cache)
    .WaitForCompletion(migrations)
    .WithHttpHealthCheck("/health");

builder.AddProject<Projects.DeliveryControlCenter_Worker>("worker")
    .WithReference(deliveryDb)
    .WithReference(cache)
    .WaitForCompletion(migrations);

builder.AddProject<Projects.DeliveryControlCenter_Web>("web")
    .WithReference(api)
    .WaitFor(api)
    .WithExternalHttpEndpoints();

await builder.Build().RunAsync();
