using Microsoft.Data.SqlClient;
using Xunit;
using Xunit.Abstractions;

namespace ShoppingListApi.Tests;

public class UnitTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public UnitTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void Check_Database_Connection()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();
            
        var connectionString = configuration.GetConnectionString("Azure");
        var canConnect = false;
        
        try
        {
            using var connection = new SqlConnection(connectionString);
            connection.Open();
            canConnect = connection.State == System.Data.ConnectionState.Open;
            connection.Close();
        }
        catch (Exception ex)
        {
            _testOutputHelper.WriteLine($"Connection error: {ex.Message}");
        }
        
        Assert.True(canConnect, "The connection to the database was successfully established!");
    }
}