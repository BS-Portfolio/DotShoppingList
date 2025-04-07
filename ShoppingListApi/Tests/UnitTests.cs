using Microsoft.Data.SqlClient;
using Moq;
using ShoppingListApi.Configs;
using ShoppingListApi.Model.Get;
using ShoppingListApi.Model.Patch;
using ShoppingListApi.Model.Post;
using ShoppingListApi.Services;
using Xunit;
using Xunit.Abstractions;

namespace ShoppingListApi.Tests;

public class UnitTests
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly Mock<ILogger<DatabaseService>> _dbLoggerMock;
    private readonly Mock<IServiceProvider> _serviceProviderMock;
    private readonly Mock<ConnectionStringService> _connectionStringServiceMock;
    private readonly DatabaseService _databaseService;

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
        
        var userMessage = canConnect ? "Connection successful!" : "Connection failed!";
        
        Assert.True(canConnect, userMessage);
    }
    [Fact]
    public void ShoppingList_AddItemsToShoppingList_AddsItems()
    {
        // Arrange
        var listId = Guid.NewGuid();
        var owner = new ListUserMinimal(Guid.NewGuid(), "John", "Doe", "john@example.com");
        var shoppingList = new ShoppingList(listId, "Grocery List", owner);
        
        var items = new List<Item>
        {
            new Item(Guid.NewGuid(), "Milk", "2 liters"),
            new Item(Guid.NewGuid(), "Bread", "1 loaf")
        };

        // Act
        shoppingList.AddItemsToShoppingList(items);

        // Assert
        Assert.Equal(2, shoppingList.Items.Count);
        Assert.Contains(items[0], shoppingList.Items);
        Assert.Contains(items[1], shoppingList.Items);
    }

    [Fact]
    public void ItemPatch_Constructor_SetsOnlySpecifiedProperties()
    {
        // Arrange
        var newName = "New Item Name";
        var newAmount = "New Amount";

        // Act
        var itemPatch = new ItemPatch(newName, newAmount);

        // Assert
        Assert.Equal(newName, itemPatch.NewItemName);
        Assert.Equal(newAmount, itemPatch.NewItemAmount);
    }

    [Fact]
    public void GenerateApiKey_ReturnsNonEmptyString()
    {
        // Act
        var apiKey = HM.GenerateApiKey();

        // Assert
        Assert.NotNull(apiKey);
        Assert.NotEmpty(apiKey);
        Assert.True(apiKey.Length > 10); // Sollte eine angemessene Länge haben
    }

    [Fact]
    public void GenerateApiKey_ReturnsDifferentKeysOnMultipleCalls()
    {
        // Act
        var key1 = HM.GenerateApiKey();
        var key2 = HM.GenerateApiKey();
        var key3 = HM.GenerateApiKey();

        // Assert
        Assert.NotEqual(key1, key2);
        Assert.NotEqual(key1, key3);
        Assert.NotEqual(key2, key3);
    }

    [Fact]
    public void UserRoleEnum_HasExpectedValues()
    {
        // Assert
        Assert.Equal(1, (int)Enums.UserRoleEnum.ListAdmin);
        Assert.Equal(2, (int)Enums.UserRoleEnum.Collaborator);
    }

    [Fact]
    public void UserRole_Constructor_SetsEnumIndex()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var roleTitle = "Admin";
        var enumIndex = (int)Enums.UserRoleEnum.ListAdmin;

        // Act
        var userRole = new UserRole(roleId, roleTitle, enumIndex);

        // Assert
        Assert.Equal(roleId, userRole.UserRoleId);
        Assert.Equal(roleTitle, userRole.UserRoleTitle);
        Assert.Equal(enumIndex, userRole.EnumIndex);
    }
}