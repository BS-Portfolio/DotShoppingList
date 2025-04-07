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
            
        var connectionString = configuration.GetConnectionString("Milad");
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
    public async Task AddUser_ValidUserPost_ReturnsSuccessAndUserId()
    {
        // Arrange
        var creationDate = DateTimeOffset.UtcNow;
        var userPost = new ListUserPost(
            "John", 
            "Doe", 
            "john.doe@example.com", 
            "Password123!", 
            creationDate);

        // Create mocks for SQL dependencies
        var mockConnection = new Mock<SqlConnection>();
        var mockCommand = new Mock<SqlCommand>();
        
        // Setup command to return 1 row affected (success)
        mockCommand.Setup(cmd => cmd.ExecuteNonQueryAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        
        mockConnection.Setup(conn => conn.CreateCommand())
            .Returns(mockCommand.Object);

        // Act
        var result = await _databaseService.AddUser(userPost, mockConnection.Object);

        // Assert
        Assert.True(result.succes);
        Assert.NotNull(result.userId);
        Assert.NotEqual(Guid.Empty, result.userId.Value);
        
        // Verify SQL command was executed with correct parameters
        mockCommand.Verify(cmd => cmd.ExecuteNonQueryAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetUserByEmailAddress_ExistingEmail_ReturnsUser()
    {
        // Arrange
        string testEmail = "test@example.com";
        var now = DateTimeOffset.UtcNow;
        
        // Create mocks for SQL dependencies
        var mockConnection = new Mock<SqlConnection>();
        var mockCommand = new Mock<SqlCommand>();
        var mockDataReader = new Mock<SqlDataReader>();
        
        // Setup data reader to return test user data
        mockDataReader.Setup(r => r.HasRows).Returns(true);
        mockDataReader.Setup(r => r.ReadAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);
        mockDataReader.Setup(r => r.GetGuid(0)).Returns(Guid.NewGuid());
        mockDataReader.Setup(r => r.GetString(1)).Returns("John");
        mockDataReader.Setup(r => r.GetString(2)).Returns("Doe");
        mockDataReader.Setup(r => r.GetString(3)).Returns(testEmail);
        mockDataReader.Setup(r => r.GetDateTime(4)).Returns(now.DateTime);
        mockDataReader.Setup(r => r.GetString(5)).Returns("apikey123");
        mockDataReader.Setup(r => r.GetDateTime(6)).Returns(now.AddDays(30).DateTime);
        
        mockCommand.Setup(cmd => cmd.ExecuteReaderAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockDataReader.Object);
        
        mockConnection.Setup(conn => conn.CreateCommand())
            .Returns(mockCommand.Object);

        // Create partial mock of DatabaseService to intercept the generic GetUser method
        var databaseServiceMock = new Mock<DatabaseService>(_serviceProviderMock.Object) { CallBase = true };
        databaseServiceMock
            .Setup(ds => ds.GetUser(
                It.IsAny<string>(), 
                It.IsAny<string>(), 
                It.IsAny<SqlConnection>()))
            .ReturnsAsync(new ListUser(
                Guid.NewGuid(), 
                "John", 
                "Doe", 
                testEmail, 
                now,
                "apikey123",
                now.AddDays(30)));

        // Act
        var result = await databaseServiceMock.Object.GetUserByEmailAddress(testEmail, mockConnection.Object);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(testEmail, result.EmailAddress);
        Assert.Equal("John", result.FirstName);
        Assert.Equal("Doe", result.LastName);
    }

    [Fact]
    public async Task UpdateShoppingList_ValidPatch_ReturnsSuccess()
    {
        // Arrange
        var listId = Guid.NewGuid();
        var listPatch = new ShoppingListPatch("Updated Shopping List");
        
        // Create mocks for SQL dependencies
        var mockConnection = new Mock<SqlConnection>();
        var mockCommand = new Mock<SqlCommand>();
        
        // Setup command to return 1 row affected (success)
        mockCommand.Setup(cmd => cmd.ExecuteNonQueryAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        
        mockConnection.Setup(conn => conn.CreateCommand())
            .Returns(mockCommand.Object);

        // Act
        var result = await _databaseService.UpdateShoppingList(listId, listPatch, mockConnection.Object);

        // Assert
        Assert.True(result);
        
        // Verify SQL command was executed
        mockCommand.Verify(cmd => cmd.ExecuteNonQueryAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
 
    [Fact]
    public void ListUserPost_Constructor_SetsProperties()
    {
        // Arrange
        var firstName = "John";
        var lastName = "Doe";
        var email = "john.doe@example.com";
        var password = "Password123!";
        DateTimeOffset creationDate = DateTimeOffset.UtcNow;

        // Act
        var userPost = new ListUserPost(firstName, lastName, email, password, creationDate);

        // Assert
        Assert.Equal(firstName, userPost.FirstName);
        Assert.Equal(lastName, userPost.LastName);
        Assert.Equal(email, userPost.EmailAddress);
        Assert.NotEqual(password, userPost.PasswordHash); // Passwort sollte gehasht sein
        Assert.Equal(creationDate, userPost.CreationDateTime);
        Assert.NotNull(userPost.ApiKey); // ApiKey sollte generiert werden
        Assert.True(userPost.ApiKeyExpirationDateTime > creationDate); // Ablaufdatum sollte in der Zukunft liegen
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