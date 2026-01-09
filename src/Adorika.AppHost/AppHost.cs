var builder = DistributedApplication.CreateBuilder(args);
{
    // This will pull the 'redis' image and run it automatically
    var redisConnection = builder.AddRedis("RedisConnection");

    // 1. Define the Postgres Server container
    var postgres = builder.AddPostgres("postgres-server");

    // 2. Add a specific database instance inside that container
    var postgresDatabase = postgres.AddDatabase("DefaultConnection");

    // API Configuration
    var api = builder.AddProject<Projects.Adorika_Api>("adorika-api")
        .WithReference(postgresDatabase)
        .WithReference(redisConnection)
        .WithHttpEndpoint(name: "api-http")
        .WithExternalHttpEndpoints();

    // UI Configuration
    builder.AddViteApp("Adorika-ui", "../Adorika.Ui")
        .WithReference(api)
        .WithHttpEndpoint(name: "ui-http")
        .WithExternalHttpEndpoints();
}

var app = builder.Build();
{
    app.Run();
}
