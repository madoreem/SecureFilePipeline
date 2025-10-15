namespace SecureFilePipeline.Shared;

public static class DbConfig
{
    public static string GetConnectionString()
    {
        var host = Environment.GetEnvironmentVariable("DB_HOST");
        var port = Environment.GetEnvironmentVariable("DB_PORT");
        var database = Environment.GetEnvironmentVariable("DB_NAME");
        var username = Environment.GetEnvironmentVariable("DB_USER");
        var password = Environment.GetEnvironmentVariable("DB_PASSWORD");

        return $"Host={host};Port={port};Database={database};Username={username};Password={password}";
    }
}