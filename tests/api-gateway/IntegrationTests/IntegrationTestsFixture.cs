namespace IntegrationTests;

public class IntegrationTestsFixture
{
    public const string TestDatabaseName = "PetApp_IntegrationTests";
    public const string TestConnectionString = 
        "Server=(localdb)\\mssqllocaldb;Database=PetApp_IntegrationTests;Integrated Security=true;";

    public static string GetConnectionString(string databaseName)
    {
        return $"Server=(localdb)\\mssqllocaldb;Database={databaseName};Integrated Security=true;";
    }
}
