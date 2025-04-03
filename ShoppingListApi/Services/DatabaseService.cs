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

    public async Task<T2> SqlConnectionHandler<T1, T2>(Func<T1, SqlConnection, Task<T2>> action,
        T1 parameter)
    {
        using SqlConnection sqlConnection = new(_connectionString);

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

    #region Connection-Handler

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

    public async Task<(bool success, Guid? id)> AddUserRole(SqlConnection sqlConnection, UserRolePost userRolePost)
    {
        string addQuery = "INSERT INTO UserRole (UserRoleID, UserRoleTitle, EnumIndex)"
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
        string addQuery =
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
        string addQuery =
            "INSERT INTO ShoppingList (ShoppingListName) "
            + "VALUES (@ShoppingListName)";

        Guid userId = Guid.NewGuid();

        List<SqlParameter> parameters =
        [
            new SqlParameter() { ParameterName = "@UserID", Value = userId, SqlDbType = SqlDbType.UniqueIdentifier },
            new SqlParameter() { ParameterName = "@FirstName", Value = shoppingListPost.ShoppingListName },
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
                nameof(DatabaseService), nameof(AddShoppingList));
            throw numberedException;
        }
    }

    public async Task<(bool success, Guid? listID)> AddItemToShoppingList(NewItemData newItemData,
        SqlConnection sqlConnection)
    {
        string addQuery =
            "INSERT INTO Item (ItemID, ShoppingListID, ItemName, ItemUnit, ItemAmount) "
            + "VALUES (@ItemID, @ShoppingListID, @ItemName, @ItemUnit, @ItemAmount)";

        Guid itemId = Guid.NewGuid();

        List<SqlParameter> parameters =
        [
            new SqlParameter() { ParameterName = "@UserID", Value = itemId, SqlDbType = SqlDbType.UniqueIdentifier },
            new SqlParameter()
            {
                ParameterName = "@ShoppingListID", Value = newItemData.ShoppingListId,
                SqlDbType = SqlDbType.UniqueIdentifier
            },
            new SqlParameter() { ParameterName = "@ItemName", Value = newItemData.itemPost.ItemName },
            new SqlParameter() { ParameterName = "@ItemUnit", Value = newItemData.itemPost.ItemUnit },
            new SqlParameter() { ParameterName = "@ItemAmount", Value = newItemData.itemPost.ItemAmount },
        ];

        try
        {
            bool successCheck = await AddRecord(addQuery, parameters, sqlConnection);

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

    public async Task<bool> AssignUserToShoppingList(UserListAssignmentData userListAssignmentDataData,
        SqlConnection sqlConnection)
    {
        string addQuery = "INSERT INTO  ListMember (ShoppingListID, UserID, UserRoleID) "
                          + "VALUES (@ShoppingListID, @UserID, @UserRoleID)";

        List<SqlParameter> parameters =
        [
            new SqlParameter()
            {
                ParameterName = "@ShoppingListID", Value = userListAssignmentDataData.ShoppingListId,
                SqlDbType = SqlDbType.UniqueIdentifier
            },
            new SqlParameter()
            {
                ParameterName = "@UserID", Value = userListAssignmentDataData.UserId,
                SqlDbType = SqlDbType.UniqueIdentifier
            },
            new SqlParameter()
            {
                ParameterName = "@UserID", Value = userListAssignmentDataData.UserRoleId,
                SqlDbType = SqlDbType.UniqueIdentifier
            }
        ];

        try
        {
            bool successCheck = await AddRecord(addQuery, parameters, sqlConnection);

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
                nameof(DatabaseService), nameof(AddItemToShoppingList));
            throw numberedException;
        }
    }

    #endregion

    #region Data-Modifier

    #endregion

    #region Data-Remover

    #endregion
}