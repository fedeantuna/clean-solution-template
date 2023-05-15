using CleanSolutionTemplate.Api;
using CleanSolutionTemplate.Application;
using CleanSolutionTemplate.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddInfrastructureServices(builder.Configuration)
    .AddPresentationServices(builder.Configuration, builder.Environment.IsDevelopment())
    .AddApplicationServices();

var app = builder.Build();

app.SetupMiddleware(builder.Environment.IsDevelopment());

app.Run();
