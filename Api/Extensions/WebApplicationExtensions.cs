public static class WebApplicationExtensions
{
    public static void EnsureDbCreatedAndSeeded(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDb>();
        db.Database.EnsureCreated();
        if (!db.Customers.Any()) DbSeeder.Seed(db);
    }
}
