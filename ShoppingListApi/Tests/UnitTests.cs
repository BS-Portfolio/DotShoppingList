using Microsoft.Data.SqlClient;
using Moq;
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
        
        Assert.True(canConnect, "The connection to the database was successfully established!");
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
}