using CleanSolutionTemplate.Api;
using CleanSolutionTemplate.Application;
using CleanSolutionTemplate.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddInfrastructureServices(builder.Configuration, false)
    .AddPresentationServices(builder.Configuration)
    .AddApplicationServices();

var app = builder.Build();

app.SetupMiddleware();

app.Run();
