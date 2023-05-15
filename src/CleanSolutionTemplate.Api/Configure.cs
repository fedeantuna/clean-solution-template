using FastEndpoints;
using FastEndpoints.Swagger;

namespace CleanSolutionTemplate.Api;

public static class Configure
{
    public static void SetupMiddleware(this WebApplication app, bool isDevelopment)
    {
        app.UseHealthChecks("/healthcheck");

        app.UseHttpsRedirection();

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseFastEndpoints();
        if (isDevelopment)
            app.UseSwaggerGen();
    }
}
