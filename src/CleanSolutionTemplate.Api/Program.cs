using CleanSolutionTemplate.Api;
using CleanSolutionTemplate.Application;
using CleanSolutionTemplate.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddPresentationServices()
    .AddApplicationServices()
    .AddInfrastructureServices(builder.Configuration);

var app = builder.Build();

app.Run();
