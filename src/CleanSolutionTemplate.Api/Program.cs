using CleanSolutionTemplate.Api;
using CleanSolutionTemplate.Application;
using CleanSolutionTemplate.Infrastructure;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddInfrastructureServices(builder.Configuration)
    .AddPresentationServices()
    .AddApplicationServices();

builder.Host
    .UseSerilog((context, services, configuration) =>
        configuration.ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .WriteTo.Console(theme: AnsiConsoleTheme.Code));

var app = builder.Build();

app.Run();
