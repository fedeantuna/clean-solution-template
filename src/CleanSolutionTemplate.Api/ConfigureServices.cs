using System.Diagnostics.CodeAnalysis;
using System.IdentityModel.Tokens.Jwt;
using CleanSolutionTemplate.Api.Services;
using CleanSolutionTemplate.Application.Common.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Authorization;

namespace CleanSolutionTemplate.Api;

public static class ConfigureServices
{
    [SuppressMessage("ReSharper", "UnusedMethodReturnValue.Global")]
    public static IServiceCollection AddPresentationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();

        services.ConfigureCors(configuration);

        services.ConfigureAuth(configuration);

        services.AddControllers(options =>
        {
            options.Filters.Add(new AuthorizeFilter());
        });

        services.AddSingleton<IUserService, UserService>();

        return services;
    }

    private static void ConfigureCors(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(builder =>
            {
                const string corsAllowedOriginsSettingName = "Cors:AllowedOrigins";
                builder.AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials()
                    .SetIsOriginAllowedToAllowWildcardSubdomains()
                    .WithOrigins(configuration.GetSection(corsAllowedOriginsSettingName).Get<string[]>());
            });
        });
    }

    private static void ConfigureAuth(this IServiceCollection services, IConfiguration configuration)
    {
        JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                const string stsAuthoritySettingName = "Sts:Authority";
                const string stsAudience = "Sts:Audience";
                options.Authority = configuration.GetValue<string>(stsAuthoritySettingName);
                options.Audience = configuration.GetValue<string>(stsAudience);
            });
        services.AddAuthorization();
    }
}
