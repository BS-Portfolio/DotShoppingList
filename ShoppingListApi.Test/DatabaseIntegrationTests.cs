using Azure.Core;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Extensions.Logging;
using ShoppingListApi.Exceptions;
using ShoppingListApi.Model.Database;
using ShoppingListApi.Model.DTOs.Get;
using ShoppingListApi.Model.DTOs.Patch;
using ShoppingListApi.Model.DTOs.Post;
using ShoppingListApi.Services;
using Xunit;
using Xunit.Abstractions;

namespace ShoppingListApi.Tests;

public class DatabaseIntegrationTests
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly ServiceProvider _serviceProvider;

    public DatabaseIntegrationTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;

        var serviceCollection = new ServiceCollection();

        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();
        serviceCollection.AddSingleton<IConfiguration>(configuration);

        var loggerFactory = LoggerFactory.Create(builder => { builder.AddNLog(); });

        serviceCollection.AddLogging();

        serviceCollection.AddSingleton<ILogger<ConnectionStringService>>(loggerFactory
            .CreateLogger<ConnectionStringService>());
        serviceCollection.AddSingleton<ILogger<DatabaseService>>(loggerFactory.CreateLogger<DatabaseService>());

        serviceCollection.AddTransient<ConnectionStringService>();

        _serviceProvider = serviceCollection.BuildServiceProvider();
    }

    [Fact]
    public void Check_Database_Connection()
    {
        _testOutputHelper.WriteLine(nameof(Check_Database_Connection));

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

        Assert.True(canConnect, "The connection to the database failed!");
    }

    [Fact]
    public void Check_Database_Connection_2()
    {
        _testOutputHelper.WriteLine(nameof(Check_Database_Connection_2));
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

        Assert.True(canConnect, "The connection to the database failed!");
    }

    [Fact]
    public async Task Check_Database_UserRoles_Existence()
    {
        try
        {
            _testOutputHelper.WriteLine(nameof(Check_Database_UserRoles_Existence));

            // Arrange
            var databaseService = new DatabaseService(_serviceProvider);

            var result = await databaseService.SqlConnectionHandlerAsync<List<UserRoleGetDto>>(
                async (connection) => await databaseService.GetUserRolesAsync(connection));

            bool hasTwo = result.Count == 2;

            bool isValid = (result.Count == 2 &&
                            result.Exists(userRole => userRole.EnumIndex == 1) &&
                            result.Exists(userRole => userRole.EnumIndex == 2)
                );
            Assert.True(hasTwo, $"The amount of user roles varies from the allowed 2! Count: {result.Count}");
            _testOutputHelper.WriteLine("Two user roles found!");
            Assert.True(isValid, "User roles vary from the allowed values!");
            _testOutputHelper.WriteLine("Both found user roles are valid!");
        }
        catch (Exception ex)
        {
            Assert.Fail($"Test failed:\nException Type: {ex.GetType()}\nMessage:{ex.Message}");
        }
    }

    [Fact]
    public async Task Check_Database_Method_AddUserAsync()
    {
        try
        {
            _testOutputHelper.WriteLine(nameof(Check_Database_Method_AddUserAsync));

            // Arrange
            var databaseService = new DatabaseService(_serviceProvider);

            var listUserPost1 = new ListUserPostDto("Milad", "Test1", "TWlsYWRAbWFzdGVyLmNvbQ==", "bWFzdGVyNjUkdWhnJg==");
            var lu1Ex = new ListUserPostExtendedDto(listUserPost1);

            var listUserPost2 = new ListUserPostDto("Milad", "Test2", "bWlsYWRAdGVzdDIuY29t", "bWFzdGVyNjUkdWhnJg==");
            var lu2Ex = new ListUserPostExtendedDto(listUserPost2);

            var listUserPost3 = new ListUserPostDto("Milad", "Test3", "bWlsYWRAdGVzdDMuY29t", "bWFzdGVyNjUkdWhnJg==");
            var lu3Ex = new ListUserPostExtendedDto(listUserPost3);

            // Act
            var result1 = await databaseService
                .TestTransactionHandlerAsync<ListUserPostExtendedDto, (bool succes, Guid? userId)>(
                    async (input, connection, tx) => await databaseService.AddUserAsync(input, connection, tx),
                    lu1Ex);


            var result2 = await databaseService
                .TestTransactionHandlerAsync<ListUserPostExtendedDto, (bool succes, Guid? userId)>(
                    async (input, connection, tx) => await databaseService.AddUserAsync(input, connection, tx),
                    lu2Ex);

            var result3 = await databaseService
                .TestTransactionHandlerAsync<ListUserPostExtendedDto, (bool succes, Guid? userId)>(
                    async (input, connection, tx) => await databaseService.AddUserAsync(input, connection, tx),
                    lu3Ex);

            // Assert
            Assert.True(result1 is { succes: true, userId: not null });
            _testOutputHelper.WriteLine("result 1 successful!");
            Assert.True(result2 is { succes: true, userId: not null });
            _testOutputHelper.WriteLine("result 2 successful!");
            Assert.True(result3 is { succes: true, userId: not null });
            _testOutputHelper.WriteLine("result 3 successful!");
        }
        catch (Exception ex)
        {
            Assert.Fail($"Test failed:\nException Type: {ex.GetType()}\nMessage:{ex.Message}");
        }
    }

    [Fact]
    public async Task Check_DatabaseService_AddShoppingListAsync()
    {
        try
        {
            _testOutputHelper.WriteLine(nameof(Check_DatabaseService_AddShoppingListAsync));

            // Arrange
            var databaseService = new DatabaseService(_serviceProvider);
            var name1 = "Rewe";
            var name2 = "Edeka";
            var name3 = "Bauhaus";

            // Act
            var result1 = await databaseService
                .TestTransactionHandlerAsync<string, (bool succes, Guid? shoppingListId)>(
                    async (input, connection, tx) => await databaseService.AddShoppingListAsync(input, connection, tx),
                    name1);
            var result2 = await databaseService
                .TestTransactionHandlerAsync<string, (bool succes, Guid? shoppingListId)>(
                    async (input, connection, tx) => await databaseService.AddShoppingListAsync(input, connection, tx),
                    name2);
            var result3 = await databaseService
                .TestTransactionHandlerAsync<string, (bool succes, Guid? shoppingListId)>(
                    async (input, connection, tx) => await databaseService.AddShoppingListAsync(input, connection, tx),
                    name3);

            //Assert

            Assert.True(result1 is { succes: true, shoppingListId: not null });
            _testOutputHelper.WriteLine("result 1 successful!");
            Assert.True(result2 is { succes: true, shoppingListId: not null });
            _testOutputHelper.WriteLine("result 2 successful!");
            Assert.True(result3 is { succes: true, shoppingListId: not null });
            _testOutputHelper.WriteLine("result 3 successful!");
        }
        catch (Exception ex)
        {
            Assert.Fail($"Test failed:\nException Type: {ex.GetType()}\nMessage:{ex.Message}");
        }
    }

    [Fact]
    public async Task Check_DatabaseService_AddItemToShoppingList()
    {
        try
        {
            _testOutputHelper.WriteLine(nameof(Check_DatabaseService_AddItemToShoppingList));

            // Arrange
            var databaseService = new DatabaseService(_serviceProvider);
            var newItemPost = new ItemPostDto("tempItem", "tempAmount");

            var listOwnerId = Guid.Parse("63ffeed9-3739-44a4-848b-447ce92b2817");
            var existingShoppingListId = Guid.Parse("484d297a-228c-4631-9a26-cd56ac2ef8ec");

            var validNewItemData = new NewItemData(newItemPost, existingShoppingListId, listOwnerId);

            var fakeShoppingListId = Guid.NewGuid();
            var fakeNewItemData = new NewItemData(newItemPost, fakeShoppingListId, listOwnerId);

            // Act
            var resultValid = await databaseService
                .TestTransactionHandlerAsync<NewItemData, (bool succes, Guid? itemId)>(
                    async (input, connection, tx) =>
                        await databaseService.AddItemToShoppingListAsync(input, connection, tx),
                    validNewItemData);

            //Assert
            Assert.True(resultValid is { succes: true, itemId: not null });

            var ex = await Assert.ThrowsAsync<NumberedException>(async () =>
            {
                var resultFake = await databaseService
                    .TestTransactionHandlerAsync<NewItemData, (bool succes, Guid? itemId)>(
                        async (input, connection, tx) =>
                            await databaseService.AddItemToShoppingListAsync(input, connection, tx),
                        fakeNewItemData);
            });

            var innerException = ex.InnerException?.InnerException;

            Assert.NotNull(innerException);
            Assert.True(innerException is SqlException);
        }
        catch (Exception ex)
        {
            Assert.Fail($"Test failed:\nException Type: {ex.GetType()}\nMessage:{ex.Message}");
        }
    }

    [Fact]
    public async Task Check_DatabaseService_ModifyShoppingListName()
    {
        try
        {
            _testOutputHelper.WriteLine(nameof(Check_DatabaseService_ModifyShoppingListName));

            // Arrange
            var databaseService = new DatabaseService(_serviceProvider);
            var existingShoppingListId = Guid.Parse("484d297a-228c-4631-9a26-cd56ac2ef8ec");

            // Act
            var resultExpectTrue = await databaseService
                .TestTransactionHandlerAsync<ModificationData<Guid, ShoppingListPatchDto>, bool>(
                    async (input, connection, tx) =>
                        await databaseService.ModifyShoppingListNameAsync(input, connection, tx),
                    new ModificationData<Guid, ShoppingListPatchDto>(existingShoppingListId,
                        new ShoppingListPatchDto("Hanky")));

            var resultExpectFalse = await databaseService
                .TestTransactionHandlerAsync<ModificationData<Guid, ShoppingListPatchDto>, bool>(
                    async (input, connection, tx) =>
                        await databaseService.ModifyShoppingListNameAsync(input, connection, tx),
                    new ModificationData<Guid, ShoppingListPatchDto>(Guid.NewGuid(),
                        new ShoppingListPatchDto("Hanky")));

            //Assert
            Assert.True(resultExpectTrue, "failed to modify existing shopping list!");
            Assert.False(resultExpectFalse,
                "a shopping list with the new random id exists in the database! Try the test again!");
        }
        catch (Exception ex)
        {
            Assert.Fail($"Test failed:\nException Type: {ex.GetType()}\nMessage:{ex.Message}");
        }
    }

    [Fact]
    public async Task Check_DatabaseService_RemoveItemFromShoppingList()
    {
        try
        {
            _testOutputHelper.WriteLine(nameof(Check_DatabaseService_RemoveItemFromShoppingList));

            // Arrange
            var databaseService = new DatabaseService(_serviceProvider);
            var listOwnerId = Guid.Parse("63ffeed9-3739-44a4-848b-447ce92b2817");
            var existingShoppingListId = Guid.Parse("484d297a-228c-4631-9a26-cd56ac2ef8ec");
            var existingItemIdInExistingShoppingList = Guid.Parse("1e829305-850b-4fce-a6eb-58c83c984645");

            var randomId = Guid.NewGuid();

            // Act

            var resultExpectTrue = await databaseService.TestTransactionHandlerAsync<ItemIdentificationData, bool>(
                async (input, connection, tx) => await databaseService.RemoveItemAsync(input, connection, tx),
                new ItemIdentificationData(listOwnerId, existingShoppingListId, existingItemIdInExistingShoppingList));

            var resultExpectFalse1 = await databaseService.TestTransactionHandlerAsync<ItemIdentificationData, bool>(
                async (input, connection, tx) => await databaseService.RemoveItemAsync(input, connection, tx),
                new ItemIdentificationData(listOwnerId, randomId, existingItemIdInExistingShoppingList));

            var resultExpectFalse2 = await databaseService.TestTransactionHandlerAsync<ItemIdentificationData, bool>(
                async (input, connection, tx) => await databaseService.RemoveItemAsync(input, connection, tx),
                new ItemIdentificationData(listOwnerId, existingShoppingListId, randomId));

            //Assert
            Assert.True(resultExpectTrue, "Failed to remove existing item with valid credentials!");
            _testOutputHelper.WriteLine("resultExpectTrue successful!");
            Assert.False(resultExpectFalse1, "Random id was unexpectedly there as an existing shopping list id!");
            _testOutputHelper.WriteLine("resultExpectFalse1 successful!");
            Assert.False(resultExpectFalse2, "Random id was unexpectedly there as an existing item id!");
            _testOutputHelper.WriteLine("resultExpectFalse2 successful!");
        }
        catch (Exception ex)
        {
            Assert.Fail($"Test failed:\nException Type: {ex.GetType()}\nMessage:{ex.Message}");
        }
    }
}