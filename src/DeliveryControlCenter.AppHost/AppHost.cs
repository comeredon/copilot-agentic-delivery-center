var builder = DistributedApplication.CreateBuilder(args);

// A persistent volume requires a stable password; Aspire's generated default changes per run.
// The secret lives in AppHost user-secrets, never in source or appsettings.
var postgresPassword = builder.AddParameter("postgres-password", secret: true);

var postgres = builder.AddPostgres("postgres", password: postgresPassword)
    .WithDataVolume("ninth-parallel-postgres-v1")
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
    .WithHttpHealthCheck("/health", endpointName: "http");

builder.AddProject<Projects.DeliveryControlCenter_Worker>("worker")
    .WithReference(deliveryDb)
    .WithReference(cache)
    .WaitForCompletion(migrations);

builder.AddProject<Projects.DeliveryControlCenter_Web>("web")
    .WithReference(api)
    .WithExternalHttpEndpoints();

await builder.Build().RunAsync();
