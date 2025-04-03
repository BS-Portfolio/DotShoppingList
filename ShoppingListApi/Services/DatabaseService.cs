using System.Data;
using ShoppingListApi.Configs;
using ShoppingListApi.Exceptions;
using ShoppingListApi.Model.Database;
using ShoppingListApi.Model.Get;
using ShoppingListApi.Model.Post;

namespace ShoppingListApi.Services;

public class DatabaseService
{
    private readonly ConnectionStringService _connectionStringService;
    private readonly ILogger<DatabaseService> _logger;
    private readonly string _connectionString;

    public DatabaseService(IServiceProvider serviceprovider)
    {
        _connectionStringService = serviceprovider.GetRequiredService<ConnectionStringService>();
        _logger = serviceprovider.GetRequiredService<ILogger<DatabaseService>>();
        _connectionString = _connectionStringService.GetConnectionString();
    }

    #region Connection-Handler

    public async Task<T2> SqlConnectionHandler<T1, T2>(Func<T1, SqlConnection, Task<T2>> action,
        T1 parameter)
    {
        await using SqlConnection sqlConnection = new(_connectionString);

        try
        {
            await sqlConnection.OpenAsync();
            var result = await action.Invoke(parameter, sqlConnection);
            return result;
        }
        catch (Exception e)
        {
            var numberedException = new NumberedException(e);
            _logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
                nameof(DatabaseService), nameof(SqlConnectionHandler));
            throw numberedException;
        }
        finally
        {
            await sqlConnection.CloseAsync();
        }
    }

    public async Task<T2> SqlConnectionHandler<T2>(Func<SqlConnection, Task<T2>> action)
    {
        using SqlConnection sqlConnection = new(_connectionString);

        try
        {
            await sqlConnection.OpenAsync();
            var result = await action.Invoke(sqlConnection);
            return result;
        }
        catch (Exception e)
        {
            var numberedException = new NumberedException(e);
            _logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
                nameof(DatabaseService), nameof(SqlConnectionHandler));
            throw numberedException;
        }
        finally
        {
            await sqlConnection.CloseAsync();
        }
    }

    #endregion


    #region Data-Reader

//     public async Task<bool> checkUserExistence(string email)
//     {
//         string checkQuery = "SELECT UserId FROM ListUser WHERE EmailAddress = @EmailAddress";
//         
//     }
// /*
//     public async Task<bool> checkShoppingListExistence()
//     {
//
//     }
//
//     public async Task<bool> checkItemExistence()
//     {
//
//     }
//
//     public async Task<bool> userRoleExistence()
//     {
//
//     }*/

    public async Task<List<UserRole>> GetUserRoles(SqlConnection sqlConnection)
    {
        List<UserRole> userRoles = [];

        string query = "SELECT UserRoleID, UserRoleTitle, EnumIndex FROM UserRole";

        await using SqlCommand sqlCommand = new(query, sqlConnection);

        try
        {
            if (sqlConnection.State != ConnectionState.Open)
            {
                await sqlConnection.OpenAsync();
            }

            await using SqlDataReader sqlReader = await sqlCommand.ExecuteReaderAsync();

            if (sqlReader.HasRows)
            {
                while (await sqlReader.ReadAsync())
                {
                    userRoles.Add(new UserRole(sqlReader.GetGuid(0), sqlReader.GetString(1), sqlReader.GetInt32(2)));
                }
            }

            sqlReader.Close();

            return userRoles;
        }
        catch (NumberedException)
        {
            throw;
        }
        catch (Exception e)
        {
            var numberedException = new NumberedException(e);
            _logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
                nameof(DatabaseService), nameof(GetUserRoles));
            throw numberedException;
        }
    }

    // generic method to get user
    // write the select pat of the quer and put the input where clasue at the end of it
    // public async Task<ListUser> GetUser<T>(T identifier, string whereClasue, SqlConnection sqlConnection)
    // {
    // }

    public async Task<ListUser?> GetUser<T>(T identifier, string whereClause, SqlConnection sqlConnection)
    {
        var query = "SELECT UserID, FirstName, LastName, EmailAddress, CreationDateTime FROM ListUser WHERE " +
                    whereClause;

        await using SqlCommand sqlCommand = new(query, sqlConnection);

        // Parameter als Liste für besseren SQL-Injection-Schutz
        List<SqlParameter> parameters = [];
    
        if (identifier != null)
        {
            parameters.Add(new SqlParameter("@Identifier", identifier));
        }
    
        sqlCommand.Parameters.AddRange(parameters.ToArray());
    
        ListUser? user = null;
    
        try
        {
            if (sqlConnection.State != ConnectionState.Open)
            {
                await sqlConnection.OpenAsync();
            }

            await using SqlDataReader sqlReader = await sqlCommand.ExecuteReaderAsync();

            if (sqlReader.HasRows && await sqlReader.ReadAsync())
            {
                // Verwendung von DateTimeOffset statt DateTime
                user = new ListUser(
                    sqlReader.GetGuid(0),
                    sqlReader.GetString(1),
                    sqlReader.GetString(2),
                    sqlReader.GetString(3),
                    DateTimeOffset.Parse(sqlReader.GetDateTime(4).ToString()) // Konvertierung zu DateTimeOffset
                );
            }
        
            // SqlReader ist bereits geschlossen durch "await using"
            return user; // Return außerhalb der if-Anweisung
        }
        catch (NumberedException)
        {
            throw;
        }
        catch (Exception e)
        {
            var numberedException = new NumberedException(e);
            _logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
                nameof(DatabaseService), nameof(GetUser));
            throw numberedException;
        }
    }

    // call GetUser<T> as GetUser<string> with email address a identifier
    // public async Task<ListUser> GetUserByEmailAddress(string emailAsdress, SqlConnection sqlConnection)
    // {
    // }

    public async Task<ListUser?> GetUserByEmailAddress(string emailAddress, SqlConnection sqlConnection)
    {
        try
        {
            return await GetUser(emailAddress, "EmailAddress = @Identifier", sqlConnection);
        }
        catch (NumberedException)
        {
            throw;
        }
        catch (Exception e)
        {
            var numberedException = new NumberedException(e);
            _logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
                nameof(DatabaseService), nameof(GetUserByEmailAddress));
            throw numberedException;
        }
    }

    // call GetUser<T> as GetUser<Guid> with userId a identifier
    // public async Task<ListUser> GetUserById(Guid userId, SqlConnection sqlConnection)
    // {
    // }
    public async Task<ListUser?> GetUserById(Guid userId, SqlConnection sqlConnection)
    {
        try
        {
            return await GetUser(userId, "UserID = @Identifier", sqlConnection);
        }
        catch (NumberedException)
        {
            throw;
        }
        catch (Exception e)
        {
            var numberedException = new NumberedException(e);
            _logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
                nameof(DatabaseService), nameof(GetUserById));
            throw numberedException;
        }
    }

    // only get the id and the name of the hopping list
    // public async Task<ShoppingList> GetShoppingListById(Guid shoppingListId, SqlConnection sqlConnection)
    // {
    // }

    public async Task<ShoppingList?> GetShoppingListById(Guid shoppingListId, SqlConnection sqlConnection)
    {
        var query = @"
        SELECT sl.ShoppingListID, sl.ShoppingListName, lu.UserID, lu.FirstName, lu.LastName, lu.EmailAddress, lu.CreationDateTime
        FROM ShoppingList sl
        JOIN ListMember lm ON sl.ShoppingListID = lm.ShoppingListID
        JOIN ListUser lu ON lm.UserID = lu.UserID
        JOIN UserRole ur ON lm.UserRoleID = ur.UserRoleID
        WHERE sl.ShoppingListID = @ShoppingListID";

        await using SqlCommand sqlCommand = new(query, sqlConnection);
        
        // Parameter als Liste für besseren SQL-Injection-Schutz
        List<SqlParameter> parameters =
        [
            new SqlParameter("@ShoppingListID", shoppingListId) { SqlDbType = SqlDbType.UniqueIdentifier }
        ];
        
        sqlCommand.Parameters.AddRange(parameters.ToArray());

        try
        {
            if (sqlConnection.State != ConnectionState.Open)
            {
                await sqlConnection.OpenAsync();
            }

            await using SqlDataReader sqlReader = await sqlCommand.ExecuteReaderAsync();

            if (sqlReader.HasRows && await sqlReader.ReadAsync())
            {
                var listOwner = new ListUser(
                    sqlReader.GetGuid(2),
                    sqlReader.GetString(3),
                    sqlReader.GetString(4),
                    sqlReader.GetString(5),
                    DateTimeOffset.Parse(sqlReader.GetDateTime(6).ToString()) // Konvertierung zu DateTimeOffset
                );

                var shoppingList = new ShoppingList(
                    sqlReader.GetGuid(0),
                    sqlReader.GetString(1),
                    listOwner
                );

                return shoppingList;
            }

            return null;
        }
        catch (NumberedException)
        {
            throw;
        }
        catch (Exception e)
        {
            var numberedException = new NumberedException(e);
            _logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
                nameof(DatabaseService), nameof(GetShoppingListById));
            throw numberedException;
        }
    }

    // only get the name and id of the shopping lists
    // public async Task<List<ShoppingList>> GetShoppingListsForUser(Guid UserId, SqlConnection sqlConnection)
    // {
    // }

    public async Task<List<ShoppingList>> GetShoppingListsForUser(Guid userId, SqlConnection sqlConnection)
    {
        var shoppingLists = new List<ShoppingList>();

        var query = @"
        SELECT sl.ShoppingListID, sl.ShoppingListName, lu.UserID, lu.FirstName, lu.LastName, lu.EmailAddress, lu.CreationDateTime
        FROM ShoppingList sl
        JOIN ListMember lm ON sl.ShoppingListID = lm.ShoppingListID
        JOIN ListUser lu ON lm.UserID = lu.UserID
        WHERE lm.UserID = @UserID";

        await using SqlCommand sqlCommand = new(query, sqlConnection);
        sqlCommand.Parameters.Add(new SqlParameter("@UserID", userId));

        try
        {
            if (sqlConnection.State != ConnectionState.Open)
            {
                await sqlConnection.OpenAsync();
            }

            await using SqlDataReader sqlReader = await sqlCommand.ExecuteReaderAsync();

            if (sqlReader.HasRows)
            {
                while (await sqlReader.ReadAsync())
                {
                    var listOwner = new ListUser(
                        sqlReader.GetGuid(2),
                        sqlReader.GetString(3),
                        sqlReader.GetString(4),
                        sqlReader.GetString(5),
                        sqlReader.GetDateTime(6)
                    );

                    var shoppingList = new ShoppingList(
                        sqlReader.GetGuid(0),
                        sqlReader.GetString(1),
                        listOwner
                    );

                    shoppingLists.Add(shoppingList);
                }
            }

            return shoppingLists;
        }
        catch (NumberedException)
        {
            throw;
        }
        catch (Exception e)
        {
            var numberedException = new NumberedException(e);
            _logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
                nameof(DatabaseService), nameof(GetShoppingListsForUser));
            throw numberedException;
        }
    }

    // public async Task<List<Item>> GetItemsForShoppingList(Guid ShoppingListId, SqlConnection sqlConnection)
    // {
    //     
    // }

    public async Task<List<Item>> GetItemsForShoppingList(Guid shoppingListId, SqlConnection sqlConnection)
    {
        var items = new List<Item>();

        // ItemUnit aus der Abfrage entfernt
        var query = "SELECT ItemID, ItemName, ItemAmount FROM Item WHERE ShoppingListID = @ShoppingListID";

        await using SqlCommand sqlCommand = new(query, sqlConnection);
    
        List<SqlParameter> parameters =
        [
            new SqlParameter("@ShoppingListID", shoppingListId) { SqlDbType = SqlDbType.UniqueIdentifier }
        ];
    
        sqlCommand.Parameters.AddRange(parameters.ToArray());

        try
        {
            if (sqlConnection.State != ConnectionState.Open)
            {
                await sqlConnection.OpenAsync();
            }

            await using SqlDataReader sqlReader = await sqlCommand.ExecuteReaderAsync();

            if (sqlReader.HasRows)
            {
                while (await sqlReader.ReadAsync())
                {
                    var item = new Item(
                        sqlReader.GetGuid(0),
                        sqlReader.GetString(1),
                        "", // Leerer string für ItemUnit
                        sqlReader.GetString(2)
                    );

                    items.Add(item);
                }
            }

            return items;
        }
        catch (NumberedException)
        {
            throw;
        }
        catch (Exception e)
        {
            var numberedException = new NumberedException(e);
            _logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
                nameof(DatabaseService), nameof(GetItemsForShoppingList));
            throw numberedException;
        }
    }

    #endregion


    #region Data-Writer

    public async Task<bool> AddRecord(string addQuery, List<SqlParameter> parameters, SqlConnection sqlConnection)
    {
        const bool success = true;

        await using SqlCommand sqlCommand = new(addQuery, sqlConnection);

        sqlCommand.Parameters.AddRange(parameters.ToArray());

        try
        {
            if (sqlConnection.State != ConnectionState.Open)
            {
                await sqlConnection.OpenAsync();
            }

            int checkResult = await sqlCommand.ExecuteNonQueryAsync();

            if (checkResult != 1)
            {
                return !success;
            }

            return success;
        }
        catch (NumberedException)
        {
            throw;
        }
        catch (Exception e)
        {
            var numberedException = new NumberedException(e);
            _logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
                nameof(DatabaseService), nameof(AddRecord));
            throw numberedException;
        }
    }

    public async Task<(bool success, Guid? userRoleId)> AddUserRole(SqlConnection sqlConnection,
        UserRolePost userRolePost)
    {
        var addQuery = "INSERT INTO UserRole (UserRoleID, UserRoleTitle, EnumIndex)"
                       + " VALUES (@UserRoleID, @UserRoleTitle, @EnumIndex)";

        Guid userRoleId = Guid.NewGuid();

        List<SqlParameter> parameters =
        [
            new SqlParameter()
                { ParameterName = "@UserRoleID", Value = userRoleId, SqlDbType = SqlDbType.UniqueIdentifier },

            new SqlParameter() { ParameterName = "@UserRoleTitle", Value = userRolePost.UserRoleTitle },
            new SqlParameter() { ParameterName = "@EnumIndex", Value = (int)userRolePost.UserRoleEnum }
        ];

        try
        {
            var successCheck = await AddRecord(addQuery, parameters, sqlConnection);

            if (successCheck is false)
            {
                return (successCheck, null);
            }

            return (successCheck, userRoleId);
        }
        catch (NumberedException)
        {
            throw;
        }
        catch (Exception e)
        {
            var numberedException = new NumberedException(e);
            _logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
                nameof(DatabaseService), nameof(AddUserRole));
            throw numberedException;
        }
    }

    public async Task<(bool succes, Guid? userId)> AddUser(ListUserPost userPost, SqlConnection sqlConnection)
    {
        var addQuery =
            "INSERT INTO ListUser (UserID, FirstName, LastName, EmailAddress, PasswordHash, CreationDateTime, ApiKey, ApiKeyExpirationTime) "
            + "VALUES (@UserID, @FirstName, @LastName, @EmailAddress, @PasswordHash, @CreationDateTime, @ApiKey, @ApiKeyExpirationTime)";

        Guid userId = Guid.NewGuid();

        List<SqlParameter> parameters =
        [
            new SqlParameter() { ParameterName = "@UserID", Value = userId, SqlDbType = SqlDbType.UniqueIdentifier },
            new SqlParameter() { ParameterName = "@FirstName", Value = userPost.FirstName },
            new SqlParameter() { ParameterName = "@LastName", Value = userPost.LastName },
            new SqlParameter() { ParameterName = "@EmailAddress", Value = userPost.EmailAddress },
            new SqlParameter() { ParameterName = "@PasswordHash", Value = userPost.PasswordHash },
            new SqlParameter() { ParameterName = "@CreationDateTime", Value = userPost.CreationDateTime },
            new SqlParameter() { ParameterName = "@ApiKey", Value = userPost.ApiKey },
            new SqlParameter() { ParameterName = "@ApiKeyExpirationTime", Value = userPost.ApiKeyExpirationDateTime }
        ];

        try
        {
            bool successCheck = await AddRecord(addQuery, parameters, sqlConnection);

            if (successCheck is false)
            {
                return (successCheck, null);
            }

            return (successCheck, userId);
        }
        catch (NumberedException)
        {
            throw;
        }
        catch (Exception e)
        {
            var numberedException = new NumberedException(e);
            _logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
                nameof(DatabaseService), nameof(AddUser));
            throw numberedException;
        }
    }

    public async Task<(bool success, Guid? listID)> AddShoppingList(ShoppingListPost shoppingListPost,
        SqlConnection sqlConnection)
    {
        var addQuery =
            "INSERT INTO ShoppingList (ShoppingListID, ShoppingListName) "
            + "VALUES (@ShoppingListID, @ShoppingListName)";

        Guid shoppingListId = Guid.NewGuid();

        List<SqlParameter> parameters =
        [
            new SqlParameter() { ParameterName = "@ShoppingListID", Value = shoppingListId, SqlDbType = SqlDbType.UniqueIdentifier },
            new SqlParameter() { ParameterName = "@ShoppingListName", Value = shoppingListPost.ShoppingListName }
        ];

        try
        {
            bool successCheck = await AddRecord(addQuery, parameters, sqlConnection);

            if (successCheck is false)
            {
                return (successCheck, null);
            }

            return (successCheck, shoppingListId);
        }
        catch (NumberedException)
        {
            throw;
        }
        catch (Exception e)
        {
            var numberedException = new NumberedException(e);
            _logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
                nameof(DatabaseService), nameof(AddShoppingList));
            throw numberedException;
        }
    }
    
    public async Task<(bool success, Guid? itemId)> AddItemToShoppingList(NewItemData newItemData,
        SqlConnection sqlConnection)
    {
        // ItemUnit aus der Abfrage entfernt
        var addQuery =
            "INSERT INTO Item (ItemID, ShoppingListID, ItemName, ItemAmount) "
            + "VALUES (@ItemID, @ShoppingListID, @ItemName, @ItemAmount)";

        Guid itemId = Guid.NewGuid();

        List<SqlParameter> parameters =
        [
            new SqlParameter() { ParameterName = "@ItemID", Value = itemId, SqlDbType = SqlDbType.UniqueIdentifier },
            new SqlParameter() { 
                ParameterName = "@ShoppingListID", 
                Value = newItemData.ShoppingListId,
                SqlDbType = SqlDbType.UniqueIdentifier 
            },
            new SqlParameter() { ParameterName = "@ItemName", Value = newItemData.itemPost.ItemName },
            new SqlParameter() { ParameterName = "@ItemAmount", Value = newItemData.itemPost.ItemAmount }
        ];

        try
        {
            var successCheck = await AddRecord(addQuery, parameters, sqlConnection);

            if (successCheck is false)
            {
                return (successCheck, null);
            }

            return (successCheck, itemId);
        }
        catch (NumberedException)
        {
            throw;
        }
        catch (Exception e)
        {
            var numberedException = new NumberedException(e);
            _logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
                nameof(DatabaseService), nameof(AddItemToShoppingList));
            throw numberedException;
        }
    }

    public async Task<bool> AssignUserToShoppingList(UserListAssignmentData userListAssignmentData,
        SqlConnection sqlConnection)
    {
        var addQuery = "INSERT INTO ListMember (ShoppingListID, UserID, UserRoleID) "
                       + "VALUES (@ShoppingListID, @UserID, @UserRoleID)";

        List<SqlParameter> parameters =
        [
            new SqlParameter() { 
                ParameterName = "@ShoppingListID", 
                Value = userListAssignmentData.ShoppingListId,
                SqlDbType = SqlDbType.UniqueIdentifier 
            },
            new SqlParameter() { 
                ParameterName = "@UserID", 
                Value = userListAssignmentData.UserId,
                SqlDbType = SqlDbType.UniqueIdentifier 
            },
            new SqlParameter() { 
                ParameterName = "@UserRoleID",  // Korrigiert von "@UserID" zu "@UserRoleID"
                Value = userListAssignmentData.UserRoleId,
                SqlDbType = SqlDbType.UniqueIdentifier 
            }
        ];

        try
        {
            var successCheck = await AddRecord(addQuery, parameters, sqlConnection);
            return successCheck;
        }
        catch (NumberedException)
        {
            throw;
        }
        catch (Exception e)
        {
            var numberedException = new NumberedException(e);
            _logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
                nameof(DatabaseService), nameof(AssignUserToShoppingList));
            throw numberedException;
        }
    }

    #endregion

    #region Data-Modifier

    #endregion

    #region Data-Remover

    #endregion
}