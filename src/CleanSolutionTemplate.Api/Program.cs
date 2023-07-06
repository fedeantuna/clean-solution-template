using CleanSolutionTemplate.Api;
using CleanSolutionTemplate.Application;
using CleanSolutionTemplate.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.SetLogging(builder.Configuration);

builder.Services
    .AddApplicationServices()
    .AddInfrastructureServices(builder.Configuration)
    .AddPresentationServices(builder.Configuration, builder.Environment.IsDevelopment());

var app = builder.Build();

app.SetupMiddleware(builder.Environment.IsDevelopment());

app.Run();
