using System.IdentityModel.Tokens.Jwt;
using CleanSolutionTemplate.Api.Services;
using CleanSolutionTemplate.Application.Common.Services;
using CleanSolutionTemplate.Infrastructure.Persistence;
using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace CleanSolutionTemplate.Api;

public static class ConfigureServices
{
    public static void AddPresentationServices(this IServiceCollection services,
        IConfiguration configuration,
        bool isDevelopment)
    {
        services.AddHttpContextAccessor();

        services.AddFastEndpoints();

        if (isDevelopment)
            services.SwaggerDocument();

        services.ConfigureAuth(configuration);

        services.AddSingleton<IUserService, UserService>();

        services.AddHealthChecks()
            .AddDbContextCheck<ApplicationDbContext>();
    }

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
