var builder = DistributedApplication.CreateBuilder(args);
{
    // This will pull the 'redis' image and run it automatically
    var redis = builder.AddRedis("RedisConnection");

    // 2. Add a specific database instance inside that container
    var postgres = builder.AddPostgres("postgres-server")
                          .WithPgAdmin().AddDatabase("adorika");

    // API Configuration
    var api = builder.AddProject<Projects.Adorika_Api>("adorika-api")
        .WithReference(postgres)
        .WithReference(redis)
        .WaitFor(postgres)
        .WaitFor(redis)
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
