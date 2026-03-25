namespace IntegrationTests;

public class IntegrationTestsFixture
{
    public const string TestDatabaseName = "PetApp_IntegrationTests";
    public const string TestConnectionString = 
        "Host=localhost;Database=PetApp_IntegrationTests;Username=postgres;Password=TestPassword123!@#;Port=5433;";

    public static string GetConnectionString(string databaseName)
    {
        return $"Host=localhost;Database={databaseName};Username=postgres;Password=TestPassword123!@#;Port=5433;";
    }
}
