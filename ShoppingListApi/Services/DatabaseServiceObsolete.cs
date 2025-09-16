using System.Data;
using ShoppingListApi.Configs;
using ShoppingListApi.Enums;
using ShoppingListApi.Exceptions;
using ShoppingListApi.Model.Database;
using ShoppingListApi.Model.DTOs.Get;
using ShoppingListApi.Model.DTOs.ObsoletePost;
using ShoppingListApi.Model.DTOs.PatchObsolete;
using ShoppingListApi.Model.ReturnTypes;

namespace ShoppingListApi.Services;

public class DatabaseServiceObsolete
{
    private readonly ConnectionStringService _connectionStringService;
    private readonly ILogger<DatabaseServiceObsolete> _logger;
    private readonly string _connectionString;

    public DatabaseServiceObsolete(IServiceProvider serviceProvider)
    {
        _connectionStringService = serviceProvider.GetRequiredService<ConnectionStringService>();
        _logger = serviceProvider.GetRequiredService<ILogger<DatabaseServiceObsolete>>();
        _connectionString = _connectionStringService.GetConnectionString();
    }

    #region Connection-Handler

    public async Task<T2> SqlConnectionHandlerAsync<T1, T2>(Func<T1, SqlConnection, Task<T2>> action,
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
                nameof(DatabaseServiceObsolete), nameof(SqlConnectionHandlerAsync));
            throw numberedException;
        }
        finally
        {
            await sqlConnection.CloseAsync();
        }
    }

    public async Task<T2> SqlConnectionHandlerAsync<T2>(Func<SqlConnection, Task<T2>> action)
    {
        await using SqlConnection sqlConnection = new(_connectionString);

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
                nameof(DatabaseServiceObsolete), nameof(SqlConnectionHandlerAsync));
            throw numberedException;
        }
        finally
        {
            await sqlConnection.CloseAsync();
        }
    }

    public async Task<T2> TestTransactionHandlerAsync<T1, T2>(Func<T1, SqlConnection, SqlTransaction, Task<T2>> action,
        T1 parameter)
    {
        await using SqlConnection sqlConnection = new(_connectionString);
        SqlTransaction? transaction = null;

        try
        {
            await sqlConnection.OpenAsync();
            transaction = sqlConnection.BeginTransaction(IsolationLevel.Snapshot);
            var result = await action.Invoke(parameter, sqlConnection, transaction);

            return result;
        }
        catch (Exception e)
        {
            var numberedException = new NumberedException(e);
            _logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
                nameof(DatabaseServiceObsolete), nameof(SqlConnectionHandlerAsync));
            throw numberedException;
        }
        finally
        {
            transaction?.Rollback();
            transaction?.Dispose();
            await sqlConnection.CloseAsync();
        }
    }

    public async Task<T2> TestTransactionHandlerAsync<T2>(Func<SqlConnection, SqlTransaction, Task<T2>> action)
    {
        await using SqlConnection sqlConnection = new(_connectionString);
        SqlTransaction? transaction = null;

        try
        {
            await sqlConnection.OpenAsync();
            transaction = sqlConnection.BeginTransaction(IsolationLevel.Snapshot);
            var result = await action.Invoke(sqlConnection, transaction);

            return result;
        }
        catch (Exception e)
        {
            var numberedException = new NumberedException(e);
            _logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
                nameof(DatabaseServiceObsolete), nameof(SqlConnectionHandlerAsync));
            throw numberedException;
        }
        finally
        {
            transaction?.Rollback();
            transaction?.Dispose();
            await sqlConnection.CloseAsync();
        }
    }

    #endregion

    #region Checkers

    [Obsolete]
    public async Task<bool> CheckUserExistenceAsync(string emailAddress, SqlConnection sqlConnection)
    {
        bool existenceCheck = true;

        string checkQuery = "SELECT UserId FROM ListUser WHERE EmailAddress = @EmailAddress";

        await using SqlCommand checkCommand = new(checkQuery, sqlConnection);

        checkCommand.Parameters.Add(new SqlParameter { ParameterName = "@EmailAddress", Value = emailAddress });

        try
        {
            if (sqlConnection.State != ConnectionState.Open)
            {
                await sqlConnection.OpenAsync();
            }

            await using SqlDataReader sqlReader = await checkCommand.ExecuteReaderAsync();

            if (sqlReader.HasRows)
            {
                existenceCheck = true;
            }
            else
            {
                existenceCheck = false;
            }

            sqlReader.Close();

            return existenceCheck;
        }
        catch (NumberedException)
        {
            throw;
        }
        catch (Exception e)
        {
            var numberedException = new NumberedException(e);
            _logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
                nameof(DatabaseServiceObsolete), nameof(CheckUserExistenceAsync));
            throw numberedException;
        }
    }

    public async Task<bool> CheckShoppingListNameExistenceAsync(ShoppingListAdditionData data,
        SqlConnection sqlConnection)
    {
        string existenceCheckQuery = "SELECT SL.ShoppingListID " +
                                     "FROM ShoppingList AS SL " +
                                     "JOIN ListMember AS LM ON SL.ShoppingListID = LM.ShoppingListID " +
                                     "WHERE SL.ShoppingListName = @ShoppingListName AND " +
                                     "LM.UserID = @UserID AND " +
                                     "LM.UserRoleID IN (SELECT UserRoleID FROM UserRole WHERE UserRole.EnumIndex = @AdminEnumIndex)";

        await using SqlCommand checkCommand = new(existenceCheckQuery, sqlConnection);

        checkCommand.Parameters.AddRange([
            new SqlParameter() { ParameterName = "@ShoppingListName", Value = data.ShoppingListName },
            new SqlParameter() { ParameterName = "@UserID", Value = data.UserId },
            new SqlParameter() { ParameterName = "@AdminEnumIndex", Value = (int)UserRoleEnum.ListOwner },
        ]);

        try
        {
            if (sqlConnection.State != ConnectionState.Open)
            {
                await sqlConnection.OpenAsync();
            }

            await using SqlDataReader sqlReader = await checkCommand.ExecuteReaderAsync();

            var existenceCheck = sqlReader.HasRows;

            sqlReader.Close();

            return existenceCheck;
        }
        catch (NumberedException)
        {
            throw;
        }
        catch (Exception e)
        {
            var numberedException = new NumberedException(e);
            _logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
                nameof(DatabaseServiceObsolete), nameof(CheckShoppingListNameExistenceAsync));
            throw numberedException;
        }
    }

    public async Task<bool> CheckShoppingListIdExistenceAsync(ShoppingListIdentificationData data,
        SqlConnection sqlConnection)
    {
        string existenceCheckQuery = "SELECT SL.ShoppingListID " +
                                     "FROM ShoppingList AS SL " +
                                     "JOIN ListMember AS LM ON SL.ShoppingListID = LM.ShoppingListID " +
                                     "JOIN UserRole AS UR ON LM.UserRoleID = UR.UserRoleID " +
                                     "WHERE SL.ShoppingListID = @ShoppingListID AND " +
                                     "LM.UserID = @UserID AND " +
                                     "LM.UserRoleID IN " +
                                     "(SELECT UserRoleID FROM UserRole WHERE UserRole.EnumIndex = @AdminUserRoleIndex)";

        await using SqlCommand checkCommand = new(existenceCheckQuery, sqlConnection);

        checkCommand.Parameters.AddRange([
            new SqlParameter() { ParameterName = "@ShoppingListID", Value = data.ShoppingListId },
            new SqlParameter() { ParameterName = "@UserID", Value = data.UserId },
            new SqlParameter() { ParameterName = "@AdminUserRoleIndex", Value = (int)UserRoleEnum.ListOwner }
        ]);

        try
        {
            if (sqlConnection.State != ConnectionState.Open)
            {
                await sqlConnection.OpenAsync();
            }

            await using SqlDataReader sqlReader = await checkCommand.ExecuteReaderAsync();

            var existenceCheck = sqlReader.HasRows;

            sqlReader.Close();

            return existenceCheck;
        }
        catch (NumberedException)
        {
            throw;
        }
        catch (Exception e)
        {
            var numberedException = new NumberedException(e);
            _logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
                nameof(DatabaseServiceObsolete), nameof(CheckShoppingListNameExistenceAsync));
            throw numberedException;
        }
    }

    [Obsolete]
    public async Task<bool> CheckUserRoleExistenceAsync(int enumIndex, SqlConnection sqlConnection)
    {
        string checkQuery = "SELECT UserRoleID FROM UserRole WHERE UserRole.EnumIndex = @EnumIndex";

        await using SqlCommand checkCommand = new(checkQuery, sqlConnection);

        checkCommand.Parameters.Add(new SqlParameter { ParameterName = "@EnumIndex", Value = enumIndex });

        try
        {
            if (sqlConnection.State != ConnectionState.Open)
            {
                await sqlConnection.OpenAsync();
            }

            await using SqlDataReader sqlReader = await checkCommand.ExecuteReaderAsync();

            var existenceCheck = sqlReader.HasRows;

            sqlReader.Close();

            return existenceCheck;
        }
        catch (NumberedException)
        {
            throw;
        }
        catch (Exception e)
        {
            var numberedException = new NumberedException(e);
            _logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
                nameof(DatabaseServiceObsolete), nameof(CheckUserRoleExistenceAsync));
            throw numberedException;
        }
    }

    public async Task<UserRoleEnum?> CheckUsersRoleInListAsync(ShoppingListIdentificationData data,
        SqlConnection sqlConnection)
    {
        int targetIndex = 0;

        string checkQuery = "SELECT UR.EnumIndex FROM ListMember AS LM " +
                            "JOIN UserRole AS UR ON UR.UserRoleID = LM.UserRoleID " +
                            "WHERE LM.UserID = @UserID AND LM.ShoppingListID = @ShoppingListID";

        await using SqlCommand checkCommand = new(checkQuery, sqlConnection);

        checkCommand.Parameters.AddRange([
            new SqlParameter { ParameterName = "@UserID", Value = data.UserId },
            new SqlParameter { ParameterName = "@ShoppingListID", Value = data.ShoppingListId },
        ]);

        try
        {
            if (sqlConnection.State != ConnectionState.Open)
            {
                await sqlConnection.OpenAsync();
            }

            await using SqlDataReader sqlReader = await checkCommand.ExecuteReaderAsync();

            if (sqlReader.HasRows is false)
            {
                return null;
            }

            while (await sqlReader.ReadAsync())
            {
                targetIndex = sqlReader.GetInt32(0);
            }

            sqlReader.Close();

            if (!Enum.IsDefined(typeof(UserRoleEnum), targetIndex))
            {
                return null;
            }

            return (UserRoleEnum)targetIndex;
        }
        catch (NumberedException)
        {
            throw;
        }
        catch (Exception e)
        {
            var numberedException = new NumberedException(e);
            _logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
                nameof(DatabaseServiceObsolete), nameof(CheckUsersRoleInListAsync));
            throw numberedException;
        }
    }

    [Obsolete]
    public async Task<CredentialsCheckReturn> CheckCredentialsAsync(LoginDataDto loginDataDto,
        SqlConnection sqlConnection)
    {
        string loginQuery =
            "SELECT UserID, PasswordHash FROM ListUser WHERE ListUser.EmailAddress = @EmailAddress";

        await using SqlCommand loginCommand = new(loginQuery, sqlConnection);

        loginCommand.Parameters.Add(new SqlParameter()
            { ParameterName = "@EmailAddress", Value = loginDataDto.EmailAddress });

        List<Guid> loadedUserIdsForEmail = [];

        string loadedPasswordHash = string.Empty;

        try
        {
            if (sqlConnection.State != ConnectionState.Open)
            {
                await sqlConnection.OpenAsync();
            }

            await using SqlDataReader sqlReader = await loginCommand.ExecuteReaderAsync();

            if (sqlReader.HasRows)
            {
                while (sqlReader.Read())
                {
                    loadedUserIdsForEmail.Add(sqlReader.GetGuid(0));
                    loadedPasswordHash = sqlReader.GetString(1);
                }
            }

            sqlReader.Close();

            if (loadedUserIdsForEmail.Count == 0)
            {
                throw new NoContentFoundException<string>("No user found for the provided email address.",
                    loginDataDto.EmailAddress);
            }

            if (loadedUserIdsForEmail.Count > 1)
            {
                throw new MultipleUsersForEmailException("The email address is registered for more than one user!",
                    loginDataDto.EmailAddress, loadedUserIdsForEmail);
            }

            bool verified = BCrypt.Net.BCrypt.EnhancedVerify(loginDataDto.Password, loadedPasswordHash);

            if (verified is false)
            {
                return new CredentialsCheckReturn(false);
            }

            var loadedUser = loadedUserIdsForEmail[0];

            return new CredentialsCheckReturn(true, loadedUser);
        }
        catch (NumberedException)
        {
            throw;
        }
        catch (Exception e)
        {
            var numberedException = new NumberedException(e);
            _logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
                nameof(DatabaseServiceObsolete), nameof(CheckCredentialsAsync));
            throw numberedException;
        }
    }

    #endregion

    #region Data-Reader

    [Obsolete]
    public async Task<List<UserRoleGetDto>> GetUserRolesAsync(SqlConnection sqlConnection)
    {
        List<UserRoleGetDto> userRoles = [];

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
                    userRoles.Add(new UserRoleGetDto(sqlReader.GetGuid(0), sqlReader.GetString(1),
                        sqlReader.GetInt32(2)));
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
                nameof(DatabaseServiceObsolete), nameof(GetUserRolesAsync));
            throw numberedException;
        }
    }

    [Obsolete]
    public async Task<ListUserGetDto?> GetUserAsync<T>(T identifier, string whereClause, SqlConnection sqlConnection)
    {
        var query =
            "SELECT UserID, FirstName, LastName, EmailAddress, CreationDateTime, ApiKey, ApiKeyExpirationDateTime FROM ListUser WHERE " +
            whereClause;

        await using SqlCommand sqlCommand = new(query, sqlConnection);

        if (identifier != null)
        {
            sqlCommand.Parameters.Add(new SqlParameter() { ParameterName = "@Identifier", Value = identifier });
        }

        ListUserGetDto? user = null;

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
                    user = new ListUserGetDto(
                        sqlReader.GetGuid(0),
                        sqlReader.GetString(1),
                        sqlReader.GetString(2),
                        sqlReader.GetString(3),
                        sqlReader.GetDateTimeOffset(4),
                        sqlReader.GetString(5),
                        sqlReader.GetDateTimeOffset(6)
                    );
                }
            }

            sqlReader.Close();

            return user;
        }
        catch (NumberedException)
        {
            throw;
        }
        catch (Exception e)
        {
            var numberedException = new NumberedException(e);
            _logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
                nameof(DatabaseServiceObsolete), nameof(GetUserAsync));
            throw numberedException;
        }
    }

    [Obsolete]
    public async Task<ListUserGetDto?> GetUserByEmailAddressAsync(string emailAddress, SqlConnection sqlConnection)
    {
        try
        {
            return await GetUserAsync(emailAddress, "EmailAddress = @Identifier", sqlConnection);
        }
        catch (NumberedException)
        {
            throw;
        }
        catch (Exception e)
        {
            var numberedException = new NumberedException(e);
            _logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
                nameof(DatabaseServiceObsolete), nameof(GetUserByEmailAddressAsync));
            throw numberedException;
        }
    }

    [Obsolete]
    public async Task<ListUserGetDto?> GetUserByIdAsync(Guid userId, SqlConnection sqlConnection)
    {
        try
        {
            return await GetUserAsync(userId, "UserID = @Identifier", sqlConnection);
        }
        catch (NumberedException)
        {
            throw;
        }
        catch (Exception e)
        {
            var numberedException = new NumberedException(e);
            _logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
                nameof(DatabaseServiceObsolete), nameof(GetUserByIdAsync));
            throw numberedException;
        }
    }

    [Obsolete]
    public async Task<Guid?> GetUserIdByEmailAsync(string emailAddress, SqlConnection sqlConnection)
    {
        List<Guid> loadedIds = [];

        string query = "SELECT UserID FROM ListUser WHERE EmailAddress = @EmailAddress";

        await using SqlCommand sqlCommand = new(query, sqlConnection);
        sqlCommand.Parameters.Add(new SqlParameter() { ParameterName = "@EmailAddress", Value = emailAddress });


        try
        {
            if (sqlConnection.State != ConnectionState.Open)
            {
                await sqlConnection.OpenAsync();
            }

            await using SqlDataReader sqlReader = await sqlCommand.ExecuteReaderAsync();

            if (sqlReader.HasRows)
            {
                while (sqlReader.Read())
                {
                    loadedIds.Add(sqlReader.GetGuid(0));
                }
            }

            sqlReader.Close();

            if (loadedIds.Count == 0)
            {
                throw new RecordNotFoundException<string>(emailAddress);
            }

            if (loadedIds.Count > 1)
            {
                throw new MultipleUsersForEmailException(emailAddress, loadedIds);
            }

            return loadedIds[0];
        }
        catch (NumberedException)
        {
            throw;
        }
        catch (Exception e)
        {
            var numberedException = new NumberedException(e);
            _logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
                nameof(DatabaseServiceObsolete), nameof(GetUserIdByEmailAsync));
            throw numberedException;
        }
    }

    public async Task<ShoppingListGetDto?> GetShoppingListByIdAsync(Guid shoppingListId, SqlConnection sqlConnection)
    {
        var query =
            "SELECT sl.ShoppingListName, lu.UserID, lu.FirstName, lu.LastName, lu.EmailAddress " +
            "FROM ShoppingList AS sl " +
            "JOIN ListMember AS lm ON sl.ShoppingListID = lm.ShoppingListID " +
            "JOIN ListUser AS lu ON lm.UserID = lu.UserID " +
            "JOIN UserRole AS ur ON lm.UserRoleID = ur.UserRoleID " +
            "WHERE sl.ShoppingListID = @ShoppingListID AND ur.EnumIndex = @AdminEnumIndex";

        await using SqlCommand sqlCommand = new(query, sqlConnection);

        sqlCommand.Parameters.AddRange([
            new SqlParameter()
                { ParameterName = "@ShoppingListID", Value = shoppingListId, SqlDbType = SqlDbType.UniqueIdentifier },
            new SqlParameter() { ParameterName = "@AdminEnumIndex", Value = (int)UserRoleEnum.ListOwner }
        ]);

        try
        {
            if (sqlConnection.State != ConnectionState.Open)
            {
                await sqlConnection.OpenAsync();
            }

            await using SqlDataReader sqlReader = await sqlCommand.ExecuteReaderAsync();

            if (sqlReader.HasRows is false)
            {
                return null;
            }

            ShoppingListGetDto? targetShoppingList = null;

            while (await sqlReader.ReadAsync())
            {
                var listOwner = new ListUserMinimalGetDto(
                    sqlReader.GetGuid(1),
                    sqlReader.GetString(2),
                    sqlReader.GetString(3),
                    sqlReader.GetString(4)
                );

                targetShoppingList = new ShoppingListGetDto(
                    shoppingListId,
                    sqlReader.GetString(0),
                    listOwner
                );
            }

            sqlReader.Close();

            return targetShoppingList;
        }
        catch (NumberedException)
        {
            throw;
        }
        catch (Exception e)
        {
            var numberedException = new NumberedException(e);
            _logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
                nameof(DatabaseServiceObsolete), nameof(GetShoppingListByIdAsync));
            throw numberedException;
        }
    }

    public async Task<List<ShoppingListGetDto>> GetShoppingListsForUserAsync(Guid userId, SqlConnection sqlConnection)
    {
        var shoppingLists = new List<ShoppingListGetDto>();

        var query =
            "SELECT sl.ShoppingListID, sl.ShoppingListName, lu.UserID, lu.FirstName, lu.LastName, lu.EmailAddress " +
            "FROM ShoppingList AS sl " +
            "JOIN ListMember AS lm ON sl.ShoppingListID = lm.ShoppingListID " +
            "JOIN ListUser AS lu ON lm.UserID = lu.UserID " +
            "JOIN UserRole AS ur ON lm.UserRoleID = ur.UserRoleID " +
            "WHERE ur.EnumIndex = @AdminEnumIndex AND sl.ShoppingListID IN " +
            "(SELECT sl.ShoppingListID " +
            "FROM ShoppingList AS sl " +
            "JOIN ListMember AS lm ON sl.ShoppingListID = lm.ShoppingListID " +
            "JOIN ListUser AS lu ON lm.UserID = lu.UserID " +
            "WHERE lm.UserID = @UserID)";

        await using SqlCommand sqlCommand = new(query, sqlConnection);
        sqlCommand.Parameters.AddRange([
            new SqlParameter() { ParameterName = "@UserID", Value = userId },
            new SqlParameter() { ParameterName = "@AdminEnumIndex", Value = (int)UserRoleEnum.ListOwner },
        ]);

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
                    var listOwner = new ListUserMinimalGetDto(
                        sqlReader.GetGuid(2),
                        sqlReader.GetString(3),
                        sqlReader.GetString(4),
                        sqlReader.GetString(5)
                    );

                    var shoppingList = new ShoppingListGetDto(
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
                nameof(DatabaseServiceObsolete), nameof(GetShoppingListsForUserAsync));
            throw numberedException;
        }
    }

    public async Task<List<ItemGetDto>> GetItemsForShoppingListAsync(Guid shoppingListId, SqlConnection sqlConnection)
    {
        var items = new List<ItemGetDto>();

        var query = "SELECT ItemID, ItemName, ItemAmount FROM Item WHERE ShoppingListID = @ShoppingListID";

        await using SqlCommand sqlCommand = new(query, sqlConnection);

        sqlCommand.Parameters.Add(new SqlParameter()
            { ParameterName = "@ShoppingListID", Value = shoppingListId, SqlDbType = SqlDbType.UniqueIdentifier });

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
                    var item = new ItemGetDto(
                        sqlReader.GetGuid(0),
                        sqlReader.GetString(1),
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
                nameof(DatabaseServiceObsolete), nameof(GetItemsForShoppingListAsync));
            throw numberedException;
        }
    }

    public async Task<List<ListUserMinimalGetDto>> GetShoppingListCollaboratorsAsync(Guid shoppingListId,
        SqlConnection sqlConnection)
    {
        List<ListUserMinimalGetDto> collaborators = [];

        string getQuery = "SELECT LU.UserID, LU.FirstName, LU.LastName, LU.EmailAddress " +
                          "FROM ListUser AS LU " +
                          "JOIN ListMember AS LM ON LU.UserID = LM.UserID " +
                          "JOIN UserRole AS UR ON UR.UserRoleID = LM.UserRoleID " +
                          "WHERE LM.ShoppingListID = @ShoppingListID AND LM.UserRoleID IN " +
                          "(SELECT UserRoleID FROM UserRole WHERE UserRole.EnumIndex = @CollaboratorEnumIndex)";

        SqlCommand sqlCommand = new(getQuery, sqlConnection);

        sqlCommand.Parameters.AddRange([
            new SqlParameter()
                { ParameterName = "@ShoppingListID", Value = shoppingListId, SqlDbType = SqlDbType.UniqueIdentifier },
            new SqlParameter() { ParameterName = "@CollaboratorEnumIndex", Value = (int)UserRoleEnum.Collaborator },
        ]);

        try
        {
            if (sqlConnection.State != ConnectionState.Open)
            {
                await sqlConnection.OpenAsync();
            }

            await using SqlDataReader sqlReader = await sqlCommand.ExecuteReaderAsync();

            if (!sqlReader.HasRows)
            {
                return collaborators;
            }

            if (sqlReader.HasRows)
            {
                while (await sqlReader.ReadAsync())
                {
                    collaborators.Add(new ListUserMinimalGetDto(
                        sqlReader.GetGuid(0),
                        sqlReader.GetString(1),
                        sqlReader.GetString(2),
                        sqlReader.GetString(3)
                    ));
                }
            }

            sqlReader.Close();

            return collaborators;
        }
        catch (NumberedException)
        {
            throw;
        }
        catch (Exception e)
        {
            var numberedException = new NumberedException(e);
            _logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
                nameof(DatabaseServiceObsolete), nameof(GetShoppingListCollaboratorsAsync));
            throw numberedException;
        }
    }

    public async Task<List<ListUserMinimalGetDto>> GetAllUsersAsync(SqlConnection sqlConnection)
    {
        List<ListUserMinimalGetDto> users = [];

        string query = "SELECT UserID, FirstName, LastName, EmailAddress " +
                       "FROM ListUser";

        await using SqlCommand command = new(query, sqlConnection);

        try
        {
            if (sqlConnection.State != ConnectionState.Open)
            {
                await sqlConnection.OpenAsync();
            }

            await using SqlDataReader sqlReader = await command.ExecuteReaderAsync();

            if (sqlReader.HasRows)
            {
                while (sqlReader.Read())
                {
                    users.Add(new ListUserMinimalGetDto(
                        sqlReader.GetGuid(0),
                        sqlReader.GetString(1),
                        sqlReader.GetString(2),
                        sqlReader.GetString(3)
                    ));
                }
            }

            sqlReader.Close();
            return users;
        }
        catch (NumberedException)
        {
            throw;
        }
        catch (Exception e)
        {
            var numberedException = new NumberedException(e);
            _logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
                nameof(DatabaseServiceObsolete), nameof(GetAllUsersAsync));
            throw numberedException;
        }
    }

    public async Task<int> GetShoppingListCountForUserAsync(Guid userId, SqlConnection sqlConnection)
    {
        int count = 0;
        string query = "SELECT COUNT(DISTINCT ShoppingListID) FROM ListMember " +
                       "WHERE UserRoleID IN (SELECT UserRoleID FROM UserRole WHERE UserRole.EnumIndex = @AdminUserRoleIndex) " +
                       "AND UserID = @UserID";

        await using SqlCommand countCommand = new(query, sqlConnection);

        countCommand.Parameters.AddRange([
            new SqlParameter() { ParameterName = "@UserID", Value = userId },
            new SqlParameter() { ParameterName = "@AdminUserRoleIndex", Value = (int)UserRoleEnum.ListOwner }
        ]);

        try
        {
            if (sqlConnection.State != ConnectionState.Open)
            {
                await sqlConnection.OpenAsync();
            }

            await using SqlDataReader sqlReader = await countCommand.ExecuteReaderAsync();

            while (sqlReader.Read())
            {
                count = sqlReader.GetInt32(0);
            }

            return count;
        }
        catch (NumberedException)
        {
            throw;
        }
        catch (Exception e)
        {
            var numberedException = new NumberedException(e);
            _logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
                nameof(DatabaseServiceObsolete), nameof(GetShoppingListCountForUserAsync));
            throw numberedException;
        }
    }

    public async Task<int> GetItemsCountForShoppingListAsync(Guid shoppingListId, SqlConnection sqlConnection)
    {
        int count = 0;
        string query = "SELECT Count(DISTINCT ItemID) FROM Item " +
                       "WHERE ShoppingListID = @ShoppingListID ";

        await using SqlCommand countCommand = new(query, sqlConnection);

        countCommand.Parameters.Add(new SqlParameter() { ParameterName = "@ShoppingListID", Value = shoppingListId });

        try
        {
            if (sqlConnection.State != ConnectionState.Open)
            {
                await sqlConnection.OpenAsync();
            }

            await using SqlDataReader sqlReader = await countCommand.ExecuteReaderAsync();

            while (sqlReader.Read())
            {
                count = sqlReader.GetInt32(0);
            }

            return count;
        }
        catch (NumberedException)
        {
            throw;
        }
        catch (Exception e)
        {
            var numberedException = new NumberedException(e);
            _logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
                nameof(DatabaseServiceObsolete), nameof(GetItemsCountForShoppingListAsync));
            throw numberedException;
        }
    }

    #endregion

    #region Multi-Handlers

    /// <summary>
    /// the input user ID should be assigned to the requesting user id not the list owner's id
    /// </summary>
    /// <param name="data"></param>
    /// <param name="sqlConnection"></param>
    /// <returns></returns>
    /// <exception cref="NumberedException"></exception>
    public async Task<RecordFetchResult<ShoppingListGetDto?>> HandleShoppingListFetchForUserAsync(
        ShoppingListIdentificationData data, SqlConnection sqlConnection)
    {
        try
        {
            var exists = await CheckShoppingListIdExistenceAsync(data, sqlConnection);

            if (exists is false)
            {
                return new RecordFetchResult<ShoppingListGetDto?>(null, null, false);
            }

            var requestingUsersRole = await CheckUsersRoleInListAsync(data, sqlConnection);

            if (requestingUsersRole is null)
            {
                return new RecordFetchResult<ShoppingListGetDto?>(null, false, true);
            }

            var shoppingList = await GetShoppingListByIdAsync(data.ShoppingListId, sqlConnection);

            if (shoppingList is null)
            {
                return new RecordFetchResult<ShoppingListGetDto?>(null, true, true);
            }

            var items = await GetItemsForShoppingListAsync(shoppingList.ShoppingListId, sqlConnection);
            shoppingList.AddItemsToShoppingList(items);

            var collaborators = await GetShoppingListCollaboratorsAsync(shoppingList.ShoppingListId, sqlConnection);
            shoppingList.AddCollaboratorsToShoppingList(collaborators);

            return new RecordFetchResult<ShoppingListGetDto?>(shoppingList, true, true);
        }
        catch (NumberedException)
        {
            throw;
        }
        catch (Exception e)
        {
            var numberedException = new NumberedException(e);
            _logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
                nameof(DatabaseServiceObsolete), nameof(HandleShoppingListsFetchForUserAsync));
            throw numberedException;
        }
    }

    public async Task<List<ShoppingListGetDto>> HandleShoppingListsFetchForUserAsync(Guid userId,
        SqlConnection sqlConnection)
    {
        try
        {
            var shoppingLists = await GetShoppingListsForUserAsync(userId, sqlConnection);

            foreach (var shoppingList in shoppingLists)
            {
                var items = await GetItemsForShoppingListAsync(shoppingList.ShoppingListId, sqlConnection);
                shoppingList.AddItemsToShoppingList(items);

                var collaborators = await GetShoppingListCollaboratorsAsync(shoppingList.ShoppingListId, sqlConnection);
                shoppingList.AddCollaboratorsToShoppingList(collaborators);
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
                nameof(DatabaseServiceObsolete), nameof(HandleShoppingListsFetchForUserAsync));
            throw numberedException;
        }
    }

    public async Task<ListUserGetDto?> HandleLoginAsync(LoginDataDto loginDataDto, SqlConnection sqlConnection)
    {
        try
        {
            var credentialsCheckResult = await CheckCredentialsAsync(loginDataDto, sqlConnection);

            if (credentialsCheckResult.LoginSuccessful is false || credentialsCheckResult.UserId is null)
            {
                return null;
            }

            var apiKeyUpdateSuccessCheck =
                await UpdateUsersApiKeyAsync((Guid)credentialsCheckResult.UserId, sqlConnection);

            if (apiKeyUpdateSuccessCheck is false)
            {
                return null;
            }

            var user = await GetUserByIdAsync((Guid)credentialsCheckResult.UserId, sqlConnection);

            return user;
        }
        catch (NumberedException)
        {
            throw;
        }
        catch (Exception e)
        {
            var numberedException = new NumberedException(e);
            _logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
                nameof(DatabaseServiceObsolete), nameof(HandleLoginAsync));
            throw numberedException;
        }
    }

    public async Task<ShoppingListAdditionResult> HandleAddingShoppingListAsync(
        ShoppingListAdditionData shoppingListAdditionData,
        SqlConnection sqlConnection)
    {
        const bool success = true;

        try
        {
            var alreadyExists = await CheckShoppingListNameExistenceAsync(
                new ShoppingListAdditionData(shoppingListAdditionData.ShoppingListName,
                    shoppingListAdditionData.UserId), sqlConnection);

            if (alreadyExists)
            {
                return new ShoppingListAdditionResult(!success, null, false, null, true);
            }

            var shoppingListCount =
                await GetShoppingListCountForUserAsync(shoppingListAdditionData.UserId, sqlConnection);

            if (shoppingListCount >= 5)
            {
                return new ShoppingListAdditionResult(!success, null, true);
            }

            var (shoppingListAdded, shoppingListId) =
                await AddShoppingListAsync(shoppingListAdditionData.ShoppingListName, sqlConnection);

            if (shoppingListAdded is false || shoppingListId is null)
            {
                return new ShoppingListAdditionResult(!success, shoppingListId, false);
            }

            var shoppingListAssigned =
                await AssignUserToShoppingListAsync(
                    new(shoppingListAdditionData.UserId, (Guid)shoppingListId, UserRoleEnum.ListOwner), sqlConnection);

            if (shoppingListAssigned is false)
            {
                return new ShoppingListAdditionResult(!success, shoppingListId, false, false);
            }

            return new ShoppingListAdditionResult(success, shoppingListId, false, true);
        }
        catch (NumberedException)
        {
            throw;
        }
        catch (Exception e)
        {
            var numberedException = new NumberedException(e);
            _logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
                nameof(DatabaseServiceObsolete), nameof(HandleAddingShoppingListAsync));
            throw numberedException;
        }
    }

    public async Task<ItemAdditionResult> HandleAddingItemToShoppingListAsync(NewItemData newItemData,
        SqlConnection sqlConnection)
    {
        const bool success = true;

        if (newItemData.RequestingUserId is null)
        {
            return new ItemAdditionResult();
        }

        try
        {
            var userRole =
                await CheckUsersRoleInListAsync(
                    new ShoppingListIdentificationData((Guid)newItemData.RequestingUserId, newItemData.ShoppingListId),
                    sqlConnection);

            if (userRole is null)
            {
                return new ItemAdditionResult(!success, false, null, false);
            }

            var result = await GetItemsCountForShoppingListAsync(newItemData.ShoppingListId, sqlConnection);

            if (result >= 20)
            {
                return new ItemAdditionResult(!success, true, null);
            }

            var (itemAdditionSuccessful, addedItemId) = await AddItemToShoppingListAsync(newItemData, sqlConnection);

            if (itemAdditionSuccessful is false || addedItemId is null)
            {
                return new ItemAdditionResult(!success, false, addedItemId);
            }

            return new ItemAdditionResult(success, false, addedItemId);
        }
        catch (NumberedException)
        {
            throw;
        }
        catch (Exception e)
        {
            var numberedException = new NumberedException(e);
            _logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
                nameof(DatabaseServiceObsolete), nameof(HandleAddingItemToShoppingListAsync));
            throw numberedException;
        }
    }

    public async Task<UpdateResult> HandleShoppingListNameUpdateAsync(
        ModificationData<(Guid userId, Guid shoppingListId), ShoppingListPatchDtoObsolete> modificationData,
        SqlConnection sqlConnection)
    {
        const bool success = true;
        const bool accessGranted = true;

        try
        {
            var userRole = await CheckUsersRoleInListAsync(
                new ShoppingListIdentificationData(modificationData.Identifier.userId,
                    modificationData.Identifier.shoppingListId),
                sqlConnection);

            if (userRole is null or not UserRoleEnum.ListOwner)
            {
                return new UpdateResult(!success, !accessGranted);
            }

            var updateSuccess = await ModifyShoppingListNameAsync(
                new ModificationData<Guid, ShoppingListPatchDtoObsolete>(modificationData.Identifier.shoppingListId,
                    modificationData.Payload),
                sqlConnection);

            if (updateSuccess is false)
            {
                return new UpdateResult(!success, accessGranted);
            }

            return new UpdateResult(success, accessGranted);
        }
        catch (NumberedException)
        {
            throw;
        }
        catch (Exception e)
        {
            var numberedException = new NumberedException(e);
            _logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
                nameof(DatabaseServiceObsolete), nameof(HandleShoppingListNameUpdateAsync));
            throw numberedException;
        }
    }

    public async Task<UpdateResult> HandleShoppingListItemUpdateAsync(
        ModificationData<(Guid userId, Guid shoppingListId, Guid itemId), ItemPatchDtoObsolete> modificationData,
        SqlConnection sqlConnection)
    {
        const bool success = true;
        const bool accessGranted = true;

        try
        {
            var userRole = await CheckUsersRoleInListAsync(
                new ShoppingListIdentificationData(modificationData.Identifier.userId,
                    modificationData.Identifier.shoppingListId),
                sqlConnection);

            if (userRole is null)
            {
                return new UpdateResult(!success, !accessGranted);
            }

            var updateSuccess = await ModifyItemAsync(
                new ModificationData<(Guid itemId, Guid shoppingListId), ItemPatchDtoObsolete>(
                    (modificationData.Identifier.itemId, modificationData.Identifier.shoppingListId),
                    modificationData.Payload),
                sqlConnection);

            if (updateSuccess is false)
            {
                return new UpdateResult(!success, accessGranted);
            }

            return new UpdateResult(success, accessGranted);
        }
        catch (NumberedException)
        {
            throw;
        }
        catch (Exception e)
        {
            var numberedException = new NumberedException(e);
            _logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
                nameof(DatabaseServiceObsolete), nameof(HandleShoppingListItemUpdateAsync));
            throw numberedException;
        }
    }

    public async Task<ShoppingListRemovalResult> HandleShoppingListRemovalAsync(ShoppingListIdentificationData data,
        SqlConnection sqlConnection)
    {
        const bool success = true;
        const bool exists = true;
        const bool accessGranted = true;

        try
        {
            var existenceCheck = await CheckShoppingListIdExistenceAsync(data, sqlConnection);

            if (existenceCheck is false)
            {
                return new ShoppingListRemovalResult(!success, !exists);
            }

            var userRole = await CheckUsersRoleInListAsync(data, sqlConnection);

            if (userRole is null or not UserRoleEnum.ListOwner)
            {
                return new(!success, exists, !accessGranted);
            }

            var removalCheck = await RemoveShoppingListByIdAsync(data.ShoppingListId, sqlConnection);

            if (removalCheck is false)
            {
                return new ShoppingListRemovalResult(!success, exists, accessGranted);
            }

            return new ShoppingListRemovalResult(success, exists, accessGranted);
        }
        catch (NumberedException)
        {
            throw;
        }
        catch (Exception e)
        {
            var numberedException = new NumberedException(e);
            _logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
                nameof(DatabaseServiceObsolete), nameof(HandleShoppingListRemovalAsync));
            throw numberedException;
        }
    }

    public async Task<UpdateResult> HandleItemRemovalFromShoppingListAsync(ItemIdentificationData data,
        SqlConnection sqlConnection)
    {
        const bool success = true;
        const bool accessGranted = true;

        try
        {
            var userRole =
                await CheckUsersRoleInListAsync(new ShoppingListIdentificationData(data.UserId, data.ShoppingListId),
                    sqlConnection);

            if (userRole is null)
            {
                return new UpdateResult(!success, !accessGranted);
            }

            var result = await RemoveItemAsync(data, sqlConnection);

            if (result is false)
            {
                return new UpdateResult(!success, accessGranted);
            }

            return new UpdateResult(success, accessGranted);
        }
        catch (NumberedException)
        {
            throw;
        }
        catch (Exception e)
        {
            var numberedException = new NumberedException(e);
            _logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
                nameof(DatabaseServiceObsolete), nameof(HandleItemRemovalFromShoppingListAsync));
            throw numberedException;
        }
    }

    public async Task<CollaboratorAddRemoveResult> HandleCollaboratorRemovalAsync(CollaboratorRemovalCheck data,
        SqlConnection sqlConnection)
    {
        try
        {
            var ownerRoleCheck = await CheckUsersRoleInListAsync(
                new ShoppingListIdentificationData(data.ListOwnerId,
                    data.ShoppingListId), sqlConnection);

            if (ownerRoleCheck is null or not UserRoleEnum.ListOwner)
            {
                return new CollaboratorAddRemoveResult(false, false);
            }

            var collaboratorRoleCheck = await CheckUsersRoleInListAsync(
                new ShoppingListIdentificationData(data.CollaboratorId, data.ShoppingListId), sqlConnection);

            if (collaboratorRoleCheck is null or not UserRoleEnum.Collaborator)
            {
                return new CollaboratorAddRemoveResult(false, true, false);
            }

            var requestIsValid = (data.ListOwnerId == data.RequestingUserId ||
                                  data.CollaboratorId == data.RequestingUserId ||
                                  data.ListOwnerId == data.CollaboratorId);

            if (requestIsValid is false)
            {
                return new CollaboratorAddRemoveResult(false, true, true, false);
            }

            var removalResult =
                await RemoveCollaboratorFromShoppingListAsync(
                    new CollaboratorRemovalData(data.ShoppingListId, data.CollaboratorId), sqlConnection);

            if (removalResult is false)
            {
                return new CollaboratorAddRemoveResult(false, true, true, true);
            }

            return new CollaboratorAddRemoveResult(true, true, true, true);
        }
        catch (NumberedException)
        {
            throw;
        }
        catch (Exception e)
        {
            var numberedException = new NumberedException(e);
            _logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
                nameof(HandleCollaboratorRemovalAsync), nameof(HandleCollaboratorRemovalAsync));
            throw numberedException;
        }
    }

    public async Task<CollaboratorAddRemoveResult> HandleAddingCollaboratorToShoppingListAsync(
        CollaboratorAdditionData data, SqlConnection sqlConnection)
    {
        try
        {
            var ownerUserRole =
                await CheckUsersRoleInListAsync(
                    new ShoppingListIdentificationData(data.ListOwnerId, data.ShoppingListId), sqlConnection);

            if (ownerUserRole is null or not UserRoleEnum.ListOwner)
            {
                return new CollaboratorAddRemoveResult(false, false);
            }

            var collaboratorId = await GetUserIdByEmailAsync(data.CollaboratorEmailAddress, sqlConnection);

            if (collaboratorId is null)
            {
                return new CollaboratorAddRemoveResult(false, true, false);
            }

            var requestIsValid = (data.ListOwnerId == data.RequestingUserId);

            if (requestIsValid is false)
            {
                return new CollaboratorAddRemoveResult(false, true, true, false);
            }

            var addResult = await AssignUserToShoppingListAsync(new UserListAssignmentData((Guid)collaboratorId,
                data.ShoppingListId, UserRoleEnum.Collaborator), sqlConnection);

            if (addResult is false)
            {
                return new CollaboratorAddRemoveResult(false, true, true, true);
            }

            return new CollaboratorAddRemoveResult(true, true, true, true);
        }
        catch (NumberedException)
        {
            throw;
        }
        catch (Exception e)
        {
            var numberedException = new NumberedException(e);
            _logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
                nameof(DatabaseServiceObsolete), nameof(HandleAddingCollaboratorToShoppingListAsync));
            throw numberedException;
        }
    }

    #endregion

    #region Data-Writer

    public async Task<bool> AddRecordAsync(string addQuery, List<SqlParameter> parameters, SqlConnection sqlConnection,
        SqlTransaction? transaction = null)
    {
        const bool success = true;

        await using SqlCommand sqlCommand = new(addQuery, sqlConnection);

        if (transaction is not null)
        {
            sqlCommand.Transaction = transaction;
        }

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
                nameof(DatabaseServiceObsolete), nameof(AddRecordAsync));
            throw numberedException;
        }
    }

    [Obsolete]
    public async Task<(bool success, Guid? userRoleId)> AddUserRoleAsync(SqlConnection sqlConnection,
        UserRolePostDto userRolePostDto)
    {
        var addQuery = "INSERT INTO UserRole (UserRoleID, UserRoleTitle, EnumIndex)"
                       + " VALUES (@UserRoleID, @UserRoleTitle, @EnumIndex)";

        Guid userRoleId = Guid.NewGuid();

        List<SqlParameter> parameters =
        [
            new SqlParameter()
                { ParameterName = "@UserRoleID", Value = userRoleId, SqlDbType = SqlDbType.UniqueIdentifier },

            new SqlParameter() { ParameterName = "@UserRoleTitle", Value = userRolePostDto.UserRoleTitle },
            new SqlParameter() { ParameterName = "@EnumIndex", Value = (int)userRolePostDto.UserRoleEnum }
        ];

        try
        {
            var successCheck = await AddRecordAsync(addQuery, parameters, sqlConnection);

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
                nameof(DatabaseServiceObsolete), nameof(AddUserRoleAsync));
            throw numberedException;
        }
    }

    [Obsolete]
    public async Task<(bool succes, Guid? userId)> AddUserAsync(ListUserPostExtendedDto userPostExtendedDto,
        SqlConnection sqlConnection, SqlTransaction? transaction = null)
    {
        var addQuery =
            "INSERT INTO ListUser (UserID, FirstName, LastName, EmailAddress, PasswordHash, CreationDateTime, ApiKey, ApiKeyExpirationDateTime) "
            + "VALUES (@UserID, @FirstName, @LastName, @EmailAddress, @PasswordHash, @CreationDateTime, @ApiKey, @ApiKeyExpirationDateTime)";

        Guid userId = Guid.NewGuid();

        List<SqlParameter> parameters =
        [
            new SqlParameter() { ParameterName = "@UserID", Value = userId, SqlDbType = SqlDbType.UniqueIdentifier },
            new SqlParameter() { ParameterName = "@FirstName", Value = userPostExtendedDto.FirstName },
            new SqlParameter() { ParameterName = "@LastName", Value = userPostExtendedDto.LastName },
            new SqlParameter() { ParameterName = "@EmailAddress", Value = userPostExtendedDto.EmailAddress },
            new SqlParameter() { ParameterName = "@PasswordHash", Value = userPostExtendedDto.PasswordHash },
            new SqlParameter() { ParameterName = "@CreationDateTime", Value = userPostExtendedDto.CreationDateTime },
            new SqlParameter() { ParameterName = "@ApiKey", Value = userPostExtendedDto.ApiKey },
            new SqlParameter()
                { ParameterName = "@ApiKeyExpirationDateTime", Value = userPostExtendedDto.ApiKeyExpirationDateTime }
        ];

        try
        {
            bool successCheck = await AddRecordAsync(addQuery, parameters, sqlConnection, transaction);

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
                nameof(DatabaseServiceObsolete), nameof(AddUserAsync));
            throw numberedException;
        }
    }

    public async Task<(bool success, Guid? listID)> AddShoppingListAsync(string shoppingListName,
        SqlConnection sqlConnection, SqlTransaction? transaction = null)
    {
        var addQuery =
            "INSERT INTO ShoppingList (ShoppingListID, ShoppingListName) "
            + "VALUES (@ShoppingListID, @ShoppingListName)";

        Guid shoppingListId = Guid.NewGuid();

        List<SqlParameter> parameters =
        [
            new SqlParameter()
                { ParameterName = "@ShoppingListID", Value = shoppingListId, SqlDbType = SqlDbType.UniqueIdentifier },
            new SqlParameter() { ParameterName = "@ShoppingListName", Value = shoppingListName }
        ];

        try
        {
            bool successCheck = await AddRecordAsync(addQuery, parameters, sqlConnection, transaction);

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
                nameof(DatabaseServiceObsolete), nameof(AddShoppingListAsync));
            throw numberedException;
        }
    }

    public async Task<(bool success, Guid? itemId)> AddItemToShoppingListAsync(NewItemData newItemData,
        SqlConnection sqlConnection, SqlTransaction? transaction = null)
    {
        var addQuery =
            "INSERT INTO Item (ItemID, ShoppingListID, ItemName, ItemAmount) "
            + "VALUES (@ItemID, @ShoppingListID, @ItemName, @ItemAmount)";

        Guid itemId = Guid.NewGuid();

        List<SqlParameter> parameters =
        [
            new SqlParameter() { ParameterName = "@ItemID", Value = itemId, SqlDbType = SqlDbType.UniqueIdentifier },
            new SqlParameter()
            {
                ParameterName = "@ShoppingListID",
                Value = newItemData.ShoppingListId,
                SqlDbType = SqlDbType.UniqueIdentifier
            },
            new SqlParameter() { ParameterName = "@ItemName", Value = newItemData.ItemPostDto.ItemName },
            new SqlParameter() { ParameterName = "@ItemAmount", Value = newItemData.ItemPostDto.ItemAmount }
        ];

        try
        {
            var successCheck = await AddRecordAsync(addQuery, parameters, sqlConnection, transaction);

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
                nameof(DatabaseServiceObsolete), nameof(AddItemToShoppingListAsync));
            throw numberedException;
        }
    }

    public async Task<bool> AssignUserToShoppingListAsync(UserListAssignmentData userListAssignmentData,
        SqlConnection sqlConnection)
    {
        var addQuery = "INSERT INTO ListMember (ShoppingListID, UserID, UserRoleID) "
                       + "VALUES (@ShoppingListID, @UserID, (SELECT UserRoleID FROM UserRole WHERE UserRole.EnumIndex = @AdminUserRoleIndex))";

        List<SqlParameter> parameters =
        [
            new SqlParameter()
            {
                ParameterName = "@ShoppingListID",
                Value = userListAssignmentData.ShoppingListId,
                SqlDbType = SqlDbType.UniqueIdentifier
            },
            new SqlParameter()
            {
                ParameterName = "@UserID",
                Value = userListAssignmentData.UserId,
                SqlDbType = SqlDbType.UniqueIdentifier
            },
            new SqlParameter()
            {
                ParameterName = "@AdminUserRoleIndex",
                Value = (int)userListAssignmentData.UserRole
            }
        ];

        try
        {
            var successCheck = await AddRecordAsync(addQuery, parameters, sqlConnection);
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
                nameof(DatabaseServiceObsolete), nameof(AssignUserToShoppingListAsync));
            throw numberedException;
        }
    }

    #endregion

    #region Data-Modifier

    public async Task<bool> ModifyUserDetailsAsync(ModificationData<Guid, ListUserPatchDtoObsolete> userDetailsModificationData,
        SqlConnection sqlConnection)
    {
        const bool success = true;
        var userId = userDetailsModificationData.Identifier;
        var userPatch = userDetailsModificationData.Payload;
        var updateParts = new List<string>();
        var parameters = new List<SqlParameter>
        {
            new() { ParameterName = "@UserID", Value = userId, SqlDbType = SqlDbType.UniqueIdentifier }
        };

        if (!string.IsNullOrEmpty(userPatch.NewFirstName))
        {
            updateParts.Add("FirstName = @FirstName");
            parameters.Add(new SqlParameter("@FirstName", userPatch.NewFirstName));
        }

        if (!string.IsNullOrEmpty(userPatch.NewLastName))
        {
            updateParts.Add("LastName = @LastName");
            parameters.Add(new SqlParameter("@LastName", userPatch.NewLastName));
        }

        if (updateParts.Count == 0)
        {
            return !success;
        }

        var query = $"UPDATE ListUser SET {string.Join(", ", updateParts)} WHERE UserID = @UserID";

        await using SqlCommand sqlCommand = new(query, sqlConnection);
        sqlCommand.Parameters.AddRange(parameters.ToArray());

        try
        {
            if (sqlConnection.State != ConnectionState.Open)
            {
                await sqlConnection.OpenAsync();
            }

            var result = await sqlCommand.ExecuteNonQueryAsync();

            if (result != 1)
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
                nameof(DatabaseServiceObsolete), nameof(ModifyUserDetailsAsync));
            throw numberedException;
        }
    }

    public async Task<bool> ModifyShoppingListNameAsync(
        ModificationData<Guid, ShoppingListPatchDtoObsolete> shoppingListModificationData,
        SqlConnection sqlConnection, SqlTransaction? transaction = null)
    {
        var listId = shoppingListModificationData.Identifier;
        var listPatch = shoppingListModificationData.Payload;
        const bool success = true;

        var query =
            "UPDATE ShoppingList SET ShoppingListName = @ShoppingListName WHERE ShoppingListID = @ShoppingListID";

        List<SqlParameter> parameters =
        [
            new SqlParameter()
                { ParameterName = "@ShoppingListID", Value = listId, SqlDbType = SqlDbType.UniqueIdentifier },
            new SqlParameter() { ParameterName = "@ShoppingListName", Value = listPatch.NewShoppingListName }
        ];

        await using SqlCommand sqlCommand = new(query, sqlConnection);
        sqlCommand.Parameters.AddRange(parameters.ToArray());

        if (transaction is not null)
        {
            sqlCommand.Transaction = transaction;
        }

        try
        {
            if (sqlConnection.State != ConnectionState.Open)
            {
                await sqlConnection.OpenAsync();
            }

            var result = await sqlCommand.ExecuteNonQueryAsync();

            if (result != 1)
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
                nameof(DatabaseServiceObsolete), nameof(ModifyShoppingListNameAsync));
            throw numberedException;
        }
    }

    public async Task<bool> ModifyItemAsync(
        ModificationData<(Guid itemId, Guid shoppingListId), ItemPatchDtoObsolete> itemModificationData,
        SqlConnection sqlConnection)
    {
        Guid itemId = itemModificationData.Identifier.itemId;
        Guid shoppingListId = itemModificationData.Identifier.shoppingListId;
        ItemPatchDtoObsolete itemPatchDtoObsolete = itemModificationData.Payload;
        const bool success = true;

        try
        {
            var updateParts = new List<string>();
            var parameters = new List<SqlParameter>
            {
                new() { ParameterName = "@ItemID", Value = itemId, SqlDbType = SqlDbType.UniqueIdentifier },
                new()
                {
                    ParameterName = "@ShoppingListID", Value = shoppingListId, SqlDbType = SqlDbType.UniqueIdentifier
                }
            };

            if (!string.IsNullOrEmpty(itemPatchDtoObsolete.NewItemName))
            {
                updateParts.Add("ItemName = @ItemName");
                parameters.Add(new SqlParameter("@ItemName", itemPatchDtoObsolete.NewItemName));
            }

            if (!string.IsNullOrEmpty(itemPatchDtoObsolete.NewItemAmount))
            {
                updateParts.Add("ItemAmount = @ItemAmount");
                parameters.Add(new SqlParameter("@ItemAmount", itemPatchDtoObsolete.NewItemAmount));
            }

            if (updateParts.Count == 0)
            {
                return success;
            }

            var query =
                $"UPDATE Item SET {string.Join(", ", updateParts)} WHERE ItemID = @ItemID AND ShoppingListID = @ShoppingListID";

            await using SqlCommand sqlCommand = new(query, sqlConnection);
            sqlCommand.Parameters.AddRange(parameters.ToArray());

            if (sqlConnection.State != ConnectionState.Open)
            {
                await sqlConnection.OpenAsync();
            }

            var result = await sqlCommand.ExecuteNonQueryAsync();

            if (result != 1)
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
                nameof(DatabaseServiceObsolete), nameof(ModifyItemAsync));
            throw numberedException;
        }
    }

    [Obsolete]
    public async Task<bool> UpdateUsersApiKeyAsync(Guid userId, SqlConnection sqlConnection)
    {
        const bool success = true;

        string updateQuery = "UPDATE ListUser " +
                             "SET ApiKey = @NewApiKey, ApiKeyExpirationDateTime = @NewExpirationDateTime " +
                             "WHERE UserID = @UserID";

        await using SqlCommand updateCommand = new(updateQuery, sqlConnection);

        string newApiKey = HM.GenerateApiKey();

        updateCommand.Parameters.AddRange([
            new SqlParameter() { ParameterName = "@NewApiKey", Value = newApiKey },
            new SqlParameter() { ParameterName = "@NewExpirationDateTime", Value = DateTimeOffset.UtcNow.AddHours(6) },
            new SqlParameter() { ParameterName = "@UserID", Value = userId, SqlDbType = SqlDbType.UniqueIdentifier }
        ]);

        try
        {
            if (sqlConnection.State != ConnectionState.Open)
            {
                await sqlConnection.OpenAsync();
            }

            int checkResult = await updateCommand.ExecuteNonQueryAsync();

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
                nameof(DatabaseServiceObsolete), nameof(UpdateUsersApiKeyAsync));
            throw numberedException;
        }
    }

    #endregion

    #region Data-Remover

    public async Task<(bool success, int removedShoppingListsCount)> RemoveUserByIdAsync(Guid userId,
        SqlConnection sqlConnection)
    {
        await using SqlCommand sqlCommand = new();

        sqlCommand.CommandType = CommandType.StoredProcedure;
        sqlCommand.CommandText = "uspRemoveUser";
        sqlCommand.Connection = sqlConnection;

        SqlParameter successParam = new SqlParameter("@success", SqlDbType.Bit)
            { Direction = ParameterDirection.Output };
        SqlParameter removedShoppingListsCountParam = new SqlParameter("@shoppingListsRemovedCount", SqlDbType.Int)
            { Direction = ParameterDirection.Output };

        sqlCommand.Parameters.AddRange([
            new SqlParameter() { ParameterName = @"userId", Value = userId, SqlDbType = SqlDbType.UniqueIdentifier },
            new SqlParameter()
            {
                ParameterName = "@listAdminRoleEnumIndex", Value = (int)UserRoleEnum.ListOwner,
                SqlDbType = SqlDbType.Int
            },
            successParam,
            removedShoppingListsCountParam
        ]);

        try
        {
            if (sqlConnection.State != ConnectionState.Open)
            {
                await sqlConnection.OpenAsync();
            }

            await sqlCommand.ExecuteNonQueryAsync();

            var procedureSuccess = (bool)successParam.Value;
            var shoppingListsRemovedCount = (int)removedShoppingListsCountParam.Value;

            return (procedureSuccess, shoppingListsRemovedCount);
        }
        catch (NumberedException)
        {
            throw;
        }
        catch (Exception e)
        {
            var numberedException = new NumberedException(e);
            _logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
                nameof(DatabaseServiceObsolete), nameof(RemoveUserByIdAsync));
            throw numberedException;
        }
    }

    public async Task<UserRemovalDbResult> RemoveUserByEmailAsync(string emailAddress,
        SqlConnection sqlConnection)
    {
        try
        {
            var userId = await GetUserIdByEmailAsync(emailAddress, sqlConnection);

            if (userId is null)
            {
                return new UserRemovalDbResult(false, false, 0);
            }

            var (success, count) = await RemoveUserByIdAsync((Guid)userId, sqlConnection);

            if (success is false)
            {
                return new UserRemovalDbResult(false, true, 0);
            }

            return new UserRemovalDbResult(success, true, count);
        }
        catch (NumberedException)
        {
            throw;
        }
        catch (Exception e)
        {
            var numberedException = new NumberedException(e);
            _logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
                nameof(DatabaseServiceObsolete), nameof(RemoveUserByEmailAsync));
            throw numberedException;
        }
    }

    public async Task<bool> RemoveItemAsync(ItemIdentificationData data, SqlConnection sqlConnection,
        SqlTransaction? transaction = null)
    {
        var query = "DELETE FROM Item WHERE ItemID = @ItemID AND ShoppingListID = @ShoppingListID";

        List<SqlParameter> parameters =
        [
            new SqlParameter()
                { ParameterName = "@ItemID", Value = data.itemId, SqlDbType = SqlDbType.UniqueIdentifier },
            new SqlParameter()
            {
                ParameterName = "@ShoppingListID", Value = data.ShoppingListId, SqlDbType = SqlDbType.UniqueIdentifier
            }
        ];

        await using SqlCommand sqlCommand = new(query, sqlConnection);
        sqlCommand.Parameters.AddRange(parameters.ToArray());

        if (transaction is not null)
        {
            sqlCommand.Transaction = transaction;
        }

        try
        {
            if (sqlConnection.State != ConnectionState.Open)
            {
                await sqlConnection.OpenAsync();
            }

            var result = await sqlCommand.ExecuteNonQueryAsync();

            return result == 1;
        }
        catch (NumberedException)
        {
            throw;
        }
        catch (Exception e)
        {
            var numberedException = new NumberedException(e);
            _logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
                nameof(DatabaseServiceObsolete), nameof(RemoveItemAsync));
            throw numberedException;
        }
    }

    public async Task<bool> RemoveCollaboratorFromShoppingListAsync(CollaboratorRemovalData collaboratorRemovalData,
        SqlConnection sqlConnection)
    {
        Guid listId = collaboratorRemovalData.ShoppingListId;
        Guid userId = collaboratorRemovalData.UserId;

        const bool success = true;

        var query = "DELETE FROM ListMember " +
                    "WHERE ShoppingListID = @ShoppingListID AND UserID = @UserID AND " +
                    "UserRoleID IN (SELECT UserRoleID FROM UserRole WHERE EnumIndex = @CollaboratorEnumIndex)";


        await using SqlCommand sqlCommand = new(query, sqlConnection);
        sqlCommand.Parameters.AddRange([
            new SqlParameter()
                { ParameterName = "@ShoppingListID", Value = listId, SqlDbType = SqlDbType.UniqueIdentifier },
            new SqlParameter()
                { ParameterName = "@UserID", Value = userId, SqlDbType = SqlDbType.UniqueIdentifier },
            new SqlParameter() { ParameterName = "CollaboratorEnumIndex", Value = (int)UserRoleEnum.Collaborator }
        ]);

        try
        {
            if (sqlConnection.State != ConnectionState.Open)
            {
                await sqlConnection.OpenAsync();
            }

            var result = await sqlCommand.ExecuteNonQueryAsync();

            if (result != 1)
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
                nameof(DatabaseServiceObsolete), nameof(RemoveCollaboratorFromShoppingListAsync));
            throw numberedException;
        }
    }

    public async Task<bool> RemoveShoppingListByIdAsync(Guid shoppingListId, SqlConnection sqlConnection)
    {
        SqlCommand sqlCommand = new();

        sqlCommand.CommandType = CommandType.StoredProcedure;
        sqlCommand.CommandText = "uspRemoveShoppingList";
        sqlCommand.Connection = sqlConnection;

        SqlParameter successParam = new SqlParameter("@success", SqlDbType.Bit)
            { Direction = ParameterDirection.Output };

        sqlCommand.Parameters.AddRange([
            new SqlParameter()
                { ParameterName = @"shoppingListId", Value = shoppingListId, SqlDbType = SqlDbType.UniqueIdentifier },
            successParam
        ]);

        try
        {
            if (sqlConnection.State != ConnectionState.Open)
            {
                await sqlConnection.OpenAsync();
            }

            await sqlCommand.ExecuteNonQueryAsync();

            var procedureSuccess = (bool)successParam.Value;

            return procedureSuccess;
        }
        catch (NumberedException)
        {
            throw;
        }
        catch (Exception e)
        {
            var numberedException = new NumberedException(e);
            _logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
                nameof(DatabaseServiceObsolete), nameof(RemoveShoppingListByIdAsync));
            throw numberedException;
        }
    }

    #endregion
}