namespace CleanSolutionTemplate.Api;

public static class Configure
{
    public static void SetupMiddleware(this WebApplication app)
    {
        app.UseHealthChecks("/health");

        app.UseHttpsRedirection();

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();
    }
}
