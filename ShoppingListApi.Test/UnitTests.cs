using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Extensions.Logging;
using ShoppingListApi.Model.Post;
using ShoppingListApi.Services;
using Xunit;
using Xunit.Abstractions;

namespace ShoppingListApi.Tests;

public class UnitTests
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly ServiceProvider _serviceProvider;

    public UnitTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;

        // Create a ServiceCollection and register necessary services
        var serviceCollection = new ServiceCollection();

        // Register Configuration
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build(); // Use your configuration
        serviceCollection.AddSingleton<IConfiguration>(configuration);
        
        // Register Logger
        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddNLog();  // Add NLog as the logging provider
        });

        serviceCollection.AddLogging();
        
        // Register ILogger<ConnectionStringService> and ILogger<DatabaseService>
        // These are provided by the LoggerFactory, not manually registered
        serviceCollection.AddSingleton<ILogger<ConnectionStringService>>(loggerFactory.CreateLogger<ConnectionStringService>());
        serviceCollection.AddSingleton<ILogger<DatabaseService>>(loggerFactory.CreateLogger<DatabaseService>());

        // Register your ConnectionStringService
        serviceCollection.AddTransient<ConnectionStringService>();

        // Build the ServiceProvider
        _serviceProvider = serviceCollection.BuildServiceProvider();
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

    [Fact]
    public void Check_Database_Connection_2()
    {
        var connectionStringService = _serviceProvider.GetRequiredService<ConnectionStringService>();

        var connectionString = connectionStringService.GetConnectionString();
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

    [Fact]
    public async Task Check_Database_Method_AddUserAsync()
    {
        // Arrange
        var databaseService = new DatabaseService(_serviceProvider);

        var listUserPost1 = new ListUserPost("Milad", "Test1", "TWlsYWRAbWFzdGVyLmNvbQ==", "bWFzdGVyNjUkdWhnJg==");
        var lu1Ex = new ListUserPostExtended(listUserPost1);
        
        var listUserPost2 = new ListUserPost("Milad", "Test2", "bWlsYWRAdGVzdDIuY29t", "bWFzdGVyNjUkdWhnJg==");
        var lu2Ex = new ListUserPostExtended(listUserPost2);
        
        var listUserPost3 = new ListUserPost("Milad", "Test3", "bWlsYWRAdGVzdDMuY29t", "bWFzdGVyNjUkdWhnJg==");
        var lu3Ex = new ListUserPostExtended(listUserPost3);
        
        // Act
        var result1 = await databaseService.TestTransactionHandlerAsync<ListUserPostExtended, (bool succes, Guid? userId)>(
            async (input, connection, tx) => await databaseService.AddUserAsync(input, connection, tx),  
            lu1Ex);
        
        var result2 = await databaseService.TestTransactionHandlerAsync<ListUserPostExtended, (bool succes, Guid? userId)>(
            async (input, connection, tx) => await databaseService.AddUserAsync(input, connection, tx),  
            lu2Ex);
        
        var result3 = await databaseService.TestTransactionHandlerAsync<ListUserPostExtended, (bool succes, Guid? userId)>(
            async (input, connection, tx) => await databaseService.AddUserAsync(input, connection, tx),  
            lu3Ex);

        // Assert
        Assert.True(result1 is { succes: true, userId: not null });
        Assert.True(result2 is { succes: true, userId: not null });
        Assert.True(result3 is { succes: true, userId: not null });
    }
}