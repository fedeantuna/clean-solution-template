using FastEndpoints;
using FastEndpoints.Swagger;

namespace CleanSolutionTemplate.Api;

public static class Configure
{
    public static void SetupMiddleware(this WebApplication app)
    {
        app.UseHealthChecks("/healthcheck");

        app.UseHttpsRedirection();

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseFastEndpoints();
        app.UseSwaggerGen();
    }
}
