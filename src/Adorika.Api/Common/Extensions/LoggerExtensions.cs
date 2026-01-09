using Serilog;
using Serilog.Core;

namespace Adorika.Api.Common.Extensions;

public static class LogBootstrap
{
    private static int _initialized;

    public static Fluent Bootstrap()
    {
        if (Log.Logger is not Logger &&
            Interlocked.Exchange(ref _initialized, 1) == 0)
        {
            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateLogger();
        }

        return new Fluent(Log.Logger);
    }

    public sealed class Fluent
    {
        private readonly Serilog.ILogger _logger;

        internal Fluent(Serilog.ILogger logger)
        {
            _logger = logger;
        }

        public Fluent Information(string messageTemplate, params object[] args)
        {
            _logger.Information(messageTemplate, args);
            return this;
        }

        public Fluent Warning(string messageTemplate, params object[] args)
        {
            _logger.Warning(messageTemplate, args);
            return this;
        }

        public Fluent Error(Exception ex, string messageTemplate, params object[] args)
        {
            _logger.Error(ex, messageTemplate, args);
            return this;
        }

        public Fluent Fatal(string messageTemplate, params object[] args)
        {
            _logger.Fatal(messageTemplate, args);
            return this;
        }

        public Fluent WithProperty(string name, object value)
        {
            return new Fluent(_logger.ForContext(name, value));
        }
    }
}
