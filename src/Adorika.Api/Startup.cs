using Adorika.Api.Common.Extensions;
using Serilog;

LogBootstrap
    .Bootstrap()
    .Information("Adorika Api booting up");
try
{
    var builder = WebApplication.CreateBuilder(args);
    {
        builder.AddApiDependencies();
    }

    var app = builder.Build();
    {
        await app.AddRequestPipeline();
        app.Run();
    }
}
catch (Exception ex) when (!ex.GetType().Name.Equals("StopTheHostException", StringComparison.Ordinal))
{
    LogBootstrap
        .Bootstrap()
        .Error(ex, "Unhandled startup exception")
        .Fatal("Adorika Api shutting down due to fatal error");

    throw;
}
finally
{
    LogBootstrap
        .Bootstrap()
        .Information("Server shutting down");

    Log.CloseAndFlush();
}
