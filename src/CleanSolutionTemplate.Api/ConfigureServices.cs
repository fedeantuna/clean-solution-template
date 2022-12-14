using System.Diagnostics.CodeAnalysis;
using System.IdentityModel.Tokens.Jwt;
using CleanSolutionTemplate.Api.SerilogPolicies;
using CleanSolutionTemplate.Api.Services;
using CleanSolutionTemplate.Application.Common.Services;
using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Serilog;
using Serilog.Core;
using Serilog.Sinks.SystemConsole.Themes;

namespace CleanSolutionTemplate.Api;

public static class ConfigureServices
{
    [SuppressMessage("ReSharper", "UnusedMethodReturnValue.Global")]
    public static IServiceCollection AddPresentationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddSerilog(CreateLogger(configuration));
        });

        services.AddHttpContextAccessor();

        services.AddFastEndpoints();
        services.AddSwaggerDoc();

        services.ConfigureAuth(configuration);

        services.AddSingleton<IUserService, UserService>();

        return services;
    }

    private static Logger CreateLogger(IConfiguration configuration) =>
        new LoggerConfiguration()
            .Destructure.UseSensitiveDataMasking()
            .ReadFrom.Configuration(configuration)
            .Enrich.FromLogContext()
            .WriteTo.Console(theme: AnsiConsoleTheme.Code)
            .CreateLogger();

    private static void ConfigureAuth(this IServiceCollection services, IConfiguration configuration)
    {
        JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                const string stsAuthoritySettingName = "Sts:Authority";
                const string stsAudience = "Sts:Audience";
                const string stsRequireHttps = "Sts:RequireHttps";
                options.Authority = configuration.GetValue<string>(stsAuthoritySettingName);
                options.Audience = configuration.GetValue<string>(stsAudience);
                options.RequireHttpsMetadata = configuration.GetValue<bool>(stsRequireHttps);
            });
        services.AddAuthorization();
    }
}
