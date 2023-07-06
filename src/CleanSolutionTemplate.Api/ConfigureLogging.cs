using CleanSolutionTemplate.Api.SerilogPolicies;
using Serilog;
using Serilog.Core;
using Serilog.Sinks.SystemConsole.Themes;

namespace CleanSolutionTemplate.Api;

public static class ConfigureLogging
{
    public static void SetLogging(this ILoggingBuilder builder, IConfiguration configuration)
    {
        builder.ClearProviders();

        builder.AddSerilog(CreateLogger(configuration));
    }

    private static Logger CreateLogger(IConfiguration configuration) =>
        new LoggerConfiguration()
            .Destructure.UseSensitiveDataMasking()
            .ReadFrom.Configuration(configuration)
            .Enrich.FromLogContext()
            .WriteTo.Console(theme: AnsiConsoleTheme.Code)
            .CreateLogger();
}
