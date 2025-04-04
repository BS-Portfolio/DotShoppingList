using System.Data;
using Azure.Core;
using Microsoft.AspNetCore.Identity.Data;
using ShoppingListApi.Configs;
using ShoppingListApi.Enums;
using ShoppingListApi.Exceptions;
using ShoppingListApi.Model.Database;
using ShoppingListApi.Model.Get;
using ShoppingListApi.Model.Post;
using ShoppingListApi.Model.Patch;
using Microsoft.Data.SqlClient;
using ShoppingListApi.Model.ReturnTypes;

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

    #region Checkers

    public async Task<bool> CheckUserExistence(string emailAddress, SqlConnection sqlConnection)
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
                nameof(DatabaseService), nameof(CheckUserExistence));
            throw numberedException;
        }
    }

    public async Task<bool> CheckShoppingListExistence(ShoppingListExistenceCheckData data, SqlConnection sqlConnection)
    {
        string existenceCheckQuery = "SELECT SL.ShoppingListID " +
                                     "FROM ShoppingList AS SL " +
                                     "JOIN MemberList AS ML ON SL.ShoppingListID = ML.ShoppingListID " +
                                     "WHERE SL.ShoppingListName = @ShoppingListName AND " +
                                     "ML.UserID = @UserID AND " +
                                     "ML.UserRoleID IN (SELECT UserRoleID FROM UserRole WHERE UserRole.EnumIndex = @AdminEnumIndex)";

        await using SqlCommand checkCommand = new(existenceCheckQuery, sqlConnection);

        checkCommand.Parameters.AddRange([
            new SqlParameter() { ParameterName = "@ShoppingListName", Value = data.ShoppingListName },
            new SqlParameter() { ParameterName = "@UserID", Value = data.UserId },
            new SqlParameter() { ParameterName = "@AdminEnumIndex", Value = (int)UserRoleEnum.ListAdmin },
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
                nameof(DatabaseService), nameof(CheckShoppingListExistence));
            throw numberedException;
        }
    }

    public async Task<bool> CheckUserRoleExistence(int enumIndex, SqlConnection sqlConnection)
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
                nameof(DatabaseService), nameof(CheckUserRoleExistence));
            throw numberedException;
        }
    }

    public async Task<UserRoleEnum?> CheckUsersRoleInList(CheckUsersRoleData data, SqlConnection sqlConnection)
    {
        int targetIndex = 0;

        string checkQuery = "SELECT UR.EnumIndex FROM ListMember AS LM " +
                            "JOIN UserRole AS UR ON UserRole.UserRoleID = ListMember.UserRoleID " +
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

            if (!sqlReader.HasRows)
            {
                return null;
            }

            while (await sqlReader.ReadAsync())
            {
                targetIndex = sqlReader.GetInt32(sqlReader.GetOrdinal("UR.EnumIndex"));
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
                nameof(DatabaseService), nameof(CheckUsersRoleInList));
            throw numberedException;
        }
    }

    #endregion

    #region Data-Reader

    public async Task<CredentialsCheckReturn> CheckCredentials(LoginData loginData, SqlConnection sqlConnection)
    {
        string loginQuery =
            "SELECT UserID, PasswordHash FROM ListUser WHERE ListUser.EmailAddress = @EmailAddress";

        await using SqlCommand loginCommand = new(loginQuery, sqlConnection);

        loginCommand.Parameters.Add(new SqlParameter()
            { ParameterName = "@EmailAddress", Value = loginData.EmailAddress });

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

            if (loadedUserIdsForEmail.Count == 0)
            {
                throw new NoContentFoundException<string>("No user found for the provided email address.",
                    loginData.EmailAddress);
            }

            if (loadedUserIdsForEmail.Count > 1)
            {
                throw new MultipleUsersForEmailException("The email address is registered for more than one user!",
                    loginData.EmailAddress, loadedUserIdsForEmail);
            }

            bool verified = BCrypt.Net.BCrypt.EnhancedVerify(loginData.Password, loadedPasswordHash);

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
                nameof(DatabaseService), nameof(CheckCredentials));
            throw numberedException;
        }
    }


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

    public async Task<ListUser?> GetUser<T>(T identifier, string whereClause, SqlConnection sqlConnection)
    {
        var query =
            "SELECT UserID, FirstName, LastName, EmailAddress, CreationDateTime, ApiKey, ApiKeyExpirationDateTime FROM ListUser WHERE " +
            whereClause;

        await using SqlCommand sqlCommand = new(query, sqlConnection);

        if (identifier != null)
        {
            sqlCommand.Parameters.Add(new SqlParameter() { ParameterName = "@Identifier", Value = identifier });
        }

        ListUser? user = null;

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
                    user = new ListUser(
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
                nameof(DatabaseService), nameof(GetUser));
            throw numberedException;
        }
    }

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

    public async Task<ShoppingList?> GetShoppingListById(Guid shoppingListId, SqlConnection sqlConnection)
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
            new SqlParameter() { ParameterName = "AdminEnumIndex", Value = (int)UserRoleEnum.ListAdmin }
        ]);

        try
        {
            if (sqlConnection.State != ConnectionState.Open)
            {
                await sqlConnection.OpenAsync();
            }

            await using SqlDataReader sqlReader = await sqlCommand.ExecuteReaderAsync();

            if (sqlReader.HasRows && await sqlReader.ReadAsync())
            {
                var listOwner = new ListUserMinimal(
                    sqlReader.GetGuid(1),
                    sqlReader.GetString(2),
                    sqlReader.GetString(3),
                    sqlReader.GetString(4)
                );

                var shoppingList = new ShoppingList(
                    shoppingListId,
                    sqlReader.GetString(0),
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

    public async Task<List<ShoppingList>> GetShoppingListsForUser(Guid userId, SqlConnection sqlConnection)
    {
        var shoppingLists = new List<ShoppingList>();

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
            new SqlParameter() { ParameterName = "@AdminEnumIndex", Value = (int)UserRoleEnum.ListAdmin },
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
                    var listOwner = new ListUserMinimal(
                        sqlReader.GetGuid(2),
                        sqlReader.GetString(3),
                        sqlReader.GetString(4),
                        sqlReader.GetString(5)
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

    public async Task<List<Item>> GetItemsForShoppingList(Guid shoppingListId, SqlConnection sqlConnection)
    {
        var items = new List<Item>();

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
                    var item = new Item(
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
                nameof(DatabaseService), nameof(GetItemsForShoppingList));
            throw numberedException;
        }
    }

    public async Task<List<ListUserMinimal>> GetShoppingListCollaborators(Guid shoppingListId,
        SqlConnection sqlConnection)
    {
        List<ListUserMinimal> collaborators = [];

        string getQuery = "SELECT LU.UserID, LU.FirstName, LU.LastName, LU.EmailAddress " +
                          "FROM ListUser AS LU " +
                          "JOIN ListMember AS LM ON ListUser.UserID = ListMember.UserID " +
                          "JOIN UserRole AS UR ON UserRole.UserRoleID = ListMember.UserRoleID " +
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
                    collaborators.Add(new ListUserMinimal(
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
                nameof(DatabaseService), nameof(GetShoppingListCollaborators));
            throw numberedException;
        }
    }

    public async Task<AuthenticationReturn> Authenticate(Guid userId, string apiKey)
    {
        const bool isVerified = true;

        string checkQuery = "SELECT ApiKey, ApiKeyExpirationDateTime " +
                            "FROM ListUser " +
                            "WHERE UserID = @UserID";

        await using SqlConnection sqlConnection = new(_connectionString);

        await using SqlCommand checkCommand = new(checkQuery, sqlConnection);
        checkCommand.Parameters.Add(new SqlParameter() { ParameterName = "@UserID", Value = userId });

        string loadedApiKey = string.Empty;
        DateTimeOffset? loadedExpirationDateTime = null;

        try
        {
            await sqlConnection.OpenAsync();
            await using SqlDataReader sqlReader = await checkCommand.ExecuteReaderAsync();

            if (sqlReader.HasRows is false)
            {
                return new AuthenticationReturn(false, !isVerified, false, false);
            }

            while (sqlReader.Read())
            {
                loadedApiKey = sqlReader.GetString(0);
                loadedExpirationDateTime = sqlReader.GetDateTimeOffset(1);
            }

            sqlReader.Close();

            if (String.IsNullOrEmpty(loadedApiKey) || loadedExpirationDateTime is null)
            {
                return new AuthenticationReturn(false, !isVerified, false, false);
            }

            if (loadedApiKey.Equals(apiKey, StringComparison.Ordinal) is false)
            {
                return new AuthenticationReturn(true, !isVerified, false, false);
            }

            if (loadedExpirationDateTime < DateTimeOffset.UtcNow)
            {
                return new AuthenticationReturn(true, !isVerified, true, false);
            }

            return new AuthenticationReturn(true, isVerified, true, true);
        }
        catch (NumberedException)
        {
            throw;
        }
        catch (Exception e)
        {
            var numberedException = new NumberedException(e);
            _logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
                nameof(DatabaseService), nameof(Authenticate));
            throw numberedException;
        }
    }

    #endregion

    #region Multi-Handlers

    public async Task<List<ShoppingList>> HandleShoppingListsFetchForUser(Guid userId, SqlConnection sqlConnection)
    {
        try
        {
            var shoppingLists = await GetShoppingListsForUser(userId, sqlConnection);

            foreach (var shoppingList in shoppingLists)
            {
                var items = await GetItemsForShoppingList(shoppingList.ShoppingListId, sqlConnection);
                shoppingList.AddItemsToShoppingList(items);

                var collaborators = await GetShoppingListCollaborators(shoppingList.ShoppingListId, sqlConnection);
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
                nameof(DatabaseService), nameof(HandleShoppingListsFetchForUser));
            throw numberedException;
        }
    }

    public async Task<ListUser?> HandleLogin(LoginData loginData, SqlConnection sqlConnection)
    {
        try
        {
            var credentialsCheckResult = await CheckCredentials(loginData, sqlConnection);

            if (credentialsCheckResult.LoginSuccessful is false || credentialsCheckResult.UserId is null)
            {
                return null;
            }

            var apiKeyUpdateSuccessCheck = await UpdateApiKey((Guid)credentialsCheckResult.UserId, sqlConnection);

            if (apiKeyUpdateSuccessCheck is false)
            {
                return null;
            }

            var user = await GetUserById((Guid)credentialsCheckResult.UserId, sqlConnection);

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
                nameof(DatabaseService), nameof(HandleLogin));
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

    public async Task<(bool succes, Guid? userId)> AddUser(ListUserPostExtended userPostExtended,
        SqlConnection sqlConnection)
    {
        var addQuery =
            "INSERT INTO ListUser (UserID, FirstName, LastName, EmailAddress, PasswordHash, CreationDateTime, ApiKey, ApiKeyExpirationDateTime) "
            + "VALUES (@UserID, @FirstName, @LastName, @EmailAddress, @PasswordHash, @CreationDateTime, @ApiKey, @ApiKeyExpirationDateTime)";

        Guid userId = Guid.NewGuid();

        List<SqlParameter> parameters =
        [
            new SqlParameter() { ParameterName = "@UserID", Value = userId, SqlDbType = SqlDbType.UniqueIdentifier },
            new SqlParameter() { ParameterName = "@FirstName", Value = userPostExtended.FirstName },
            new SqlParameter() { ParameterName = "@LastName", Value = userPostExtended.LastName },
            new SqlParameter() { ParameterName = "@EmailAddress", Value = userPostExtended.EmailAddress },
            new SqlParameter() { ParameterName = "@PasswordHash", Value = userPostExtended.PasswordHash },
            new SqlParameter() { ParameterName = "@CreationDateTime", Value = userPostExtended.CreationDateTime },
            new SqlParameter() { ParameterName = "@ApiKey", Value = userPostExtended.ApiKey },
            new SqlParameter()
                { ParameterName = "@ApiKeyExpirationDateTime", Value = userPostExtended.ApiKeyExpirationDateTime }
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
            new SqlParameter()
                { ParameterName = "@ShoppingListID", Value = shoppingListId, SqlDbType = SqlDbType.UniqueIdentifier },
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
            new SqlParameter()
            {
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
                ParameterName = "@UserRoleID", // Korrigiert von "@UserID" zu "@UserRoleID"
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

    // Aktualisieren eines Benutzers
    public async Task<bool> UpdateUser(Guid userId, ListUserPatch userPatch, SqlConnection sqlConnection)
    {
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

        if (!string.IsNullOrEmpty(userPatch.NewEmailAddress))
        {
            updateParts.Add("EmailAddress = @EmailAddress");
            parameters.Add(new SqlParameter("@EmailAddress", userPatch.NewEmailAddress));
        }

        if (updateParts.Count == 0)
        {
            return true; // Nichts zu aktualisieren
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

            return result > 0;
        }
        catch (NumberedException)
        {
            throw;
        }
        catch (Exception e)
        {
            var numberedException = new NumberedException(e);
            _logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
                nameof(DatabaseService), nameof(UpdateUser));
            throw numberedException;
        }
    }

    // Aktualisieren einer Einkaufsliste
    public async Task<bool> UpdateShoppingList(Guid listId, ShoppingListPatch listPatch,
        SqlConnection sqlConnection)
    {
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

        try
        {
            if (sqlConnection.State != ConnectionState.Open)
            {
                await sqlConnection.OpenAsync();
            }

            var result = await sqlCommand.ExecuteNonQueryAsync();

            return result > 0;
        }
        catch (NumberedException)
        {
            throw;
        }
        catch (Exception e)
        {
            var numberedException = new NumberedException(e);
            _logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
                nameof(DatabaseService), nameof(UpdateShoppingList));
            throw numberedException;
        }
    }

    // Aktualisieren eines Artikels
    public async Task<bool> UpdateItem(Guid itemId, ItemPatch itemPatch, SqlConnection sqlConnection)
    {
        try
        {
            var updateParts = new List<string>();
            var parameters = new List<SqlParameter>
            {
                new() { ParameterName = "@ItemID", Value = itemId, SqlDbType = SqlDbType.UniqueIdentifier }
            };

            if (!string.IsNullOrEmpty(itemPatch.NewItemName))
            {
                updateParts.Add("ItemName = @ItemName");
                parameters.Add(new SqlParameter("@ItemName", itemPatch.NewItemName));
            }

            if (!string.IsNullOrEmpty(itemPatch.NewItemAmount))
            {
                updateParts.Add("ItemAmount = @ItemAmount");
                parameters.Add(new SqlParameter("@ItemAmount", itemPatch.NewItemAmount));
            }

            if (updateParts.Count == 0)
            {
                return true; // Nichts zu aktualisieren
            }

            var query = $"UPDATE Item SET {string.Join(", ", updateParts)} WHERE ItemID = @ItemID";

            await using SqlCommand sqlCommand = new(query, sqlConnection);
            sqlCommand.Parameters.AddRange(parameters.ToArray());

            if (sqlConnection.State != ConnectionState.Open)
            {
                await sqlConnection.OpenAsync();
            }

            var result = await sqlCommand.ExecuteNonQueryAsync();

            return result > 0;
        }
        catch (NumberedException)
        {
            throw;
        }
        catch (Exception e)
        {
            var numberedException = new NumberedException(e);
            _logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
                nameof(DatabaseService), nameof(UpdateItem));
            throw numberedException;
        }
    }

    public async Task<bool> UpdateApiKey(Guid userId, SqlConnection sqlConnectin)
    {
        const bool success = true;

        string updateQuery = "UPDATE ListUser " +
                             "SET ApiKey = @NewApiKey, ApiKeyExpirationDateTime = @NewExpirationDateTime " +
                             "WHERE UserID = @UserID";

        await using SqlCommand updateCommand = new(updateQuery, sqlConnectin);

        string newApiKey = HM.GenerateApiKey();

        updateCommand.Parameters.AddRange([
            new SqlParameter() { ParameterName = "@NewApiKey", Value = newApiKey },
            new SqlParameter() { ParameterName = "@NewExpirationDateTime", Value = DateTimeOffset.UtcNow },
            new SqlParameter() { ParameterName = "@UserID", Value = userId, SqlDbType = SqlDbType.UniqueIdentifier }
        ]);

        try
        {
            if (sqlConnectin.State != ConnectionState.Open)
            {
                await sqlConnectin.OpenAsync();
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
                nameof(DatabaseService), nameof(UpdateApiKey));
            throw numberedException;
        }
    }

    // nicht erwuenscht!
    // Aktualisieren einer Benutzerrolle in einer Einkaufsliste
    /*public async Task<bool> UpdateUserRoleInShoppingList(Guid listId, Guid userId, Guid newRoleId,
        SqlConnection sqlConnection)
    {
        var query =
            "UPDATE ListMember SET UserRoleID = @UserRoleID WHERE ShoppingListID = @ShoppingListID AND UserID = @UserID";

        List<SqlParameter> parameters =
        [
            new SqlParameter()
                { ParameterName = "@ShoppingListID", Value = listId, SqlDbType = SqlDbType.UniqueIdentifier },
            new SqlParameter() { ParameterName = "@UserID", Value = userId, SqlDbType = SqlDbType.UniqueIdentifier },
            new SqlParameter()
                { ParameterName = "@UserRoleID", Value = newRoleId, SqlDbType = SqlDbType.UniqueIdentifier }
        ];

        try
        {
            await using SqlCommand sqlCommand = new(query, sqlConnection);
            sqlCommand.Parameters.AddRange(parameters.ToArray());

            if (sqlConnection.State != ConnectionState.Open)
            {
                await sqlConnection.OpenAsync();
            }

            var result = await sqlCommand.ExecuteNonQueryAsync();

            return result > 0;
        }
        catch (NumberedException)
        {
            throw;
        }
        catch (Exception e)
        {
            var numberedException = new NumberedException(e);
            _logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
                nameof(DatabaseService), nameof(UpdateUserRoleInShoppingList));
            throw numberedException;
        }
    }*/

    #endregion

    #region Data-Remover

    // Löschen eines Benutzers
    // klappt nicht! die Listen und ihre items muessen auch geloescht werden. Wie gesagt -> Procedures!
    /*public async Task<bool> DeleteUser(Guid userId, SqlConnection sqlConnection)
    {
        try
        {
            if (sqlConnection.State != ConnectionState.Open)
            {
                await sqlConnection.OpenAsync();
            }

            // Beginne eine Transaktion, um sicherzustellen, dass alle Löschvorgänge erfolgreich sind
            await using SqlTransaction transaction = (SqlTransaction)await sqlConnection.BeginTransactionAsync();

            try
            {
                var deleteListMemberQuery = "DELETE FROM ListMember WHERE UserID = @UserID";

                await using SqlCommand listMemberCommand = new(deleteListMemberQuery, sqlConnection, transaction);
                listMemberCommand.Parameters.Add(new SqlParameter("@UserID", userId)
                    { SqlDbType = SqlDbType.UniqueIdentifier });

                await listMemberCommand.ExecuteNonQueryAsync();

                // Lösche den Benutzer
                var deleteUserQuery = "DELETE FROM ListUser WHERE UserID = @UserID";

                await using SqlCommand userCommand = new(deleteUserQuery, sqlConnection, transaction);
                userCommand.Parameters.Add(new SqlParameter("@UserID", userId)
                    { SqlDbType = SqlDbType.UniqueIdentifier });

                int result = await userCommand.ExecuteNonQueryAsync();

                await transaction.CommitAsync();

                return result > 0;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        catch (NumberedException)
        {
            throw;
        }
        catch (Exception e)
        {
            var numberedException = new NumberedException(e);
            _logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
                nameof(DatabaseService), nameof(DeleteUser));
            throw numberedException;
        }
    }*/

    // Löschen einer Einkaufsliste
    // -> Procedures! Ich bin schon dran!
    /*public async Task<bool> DeleteShoppingList(Guid listId, SqlConnection sqlConnection)
    {
        try
        {
            if (sqlConnection.State != ConnectionState.Open)
            {
                await sqlConnection.OpenAsync();
            }

            // Beginne eine Transaktion, um sicherzustellen, dass alle Löschvorgänge erfolgreich sind
            await using SqlTransaction transaction = (SqlTransaction)await sqlConnection.BeginTransactionAsync();

            try
            {
                var deleteItemsQuery = "DELETE FROM Item WHERE ShoppingListID = @ShoppingListID";

                await using SqlCommand itemsCommand = new(deleteItemsQuery, sqlConnection, transaction);
                itemsCommand.Parameters.Add(new SqlParameter("@ShoppingListID", listId)
                    { SqlDbType = SqlDbType.UniqueIdentifier });

                await itemsCommand.ExecuteNonQueryAsync();

                // Lösche alle Benutzer-Einkaufslisten-Verknüpfungen
                var deleteListMembersQuery = "DELETE FROM ListMember WHERE ShoppingListID = @ShoppingListID";

                await using SqlCommand listMembersCommand = new(deleteListMembersQuery, sqlConnection, transaction);
                listMembersCommand.Parameters.Add(new SqlParameter("@ShoppingListID", listId)
                    { SqlDbType = SqlDbType.UniqueIdentifier });

                await listMembersCommand.ExecuteNonQueryAsync();

                var deleteListQuery = "DELETE FROM ShoppingList WHERE ShoppingListID = @ShoppingListID";

                await using SqlCommand listCommand = new(deleteListQuery, sqlConnection, transaction);
                listCommand.Parameters.Add(new SqlParameter("@ShoppingListID", listId)
                    { SqlDbType = SqlDbType.UniqueIdentifier });

                var result = await listCommand.ExecuteNonQueryAsync();

                await transaction.CommitAsync();

                return result > 0;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        catch (NumberedException)
        {
            throw;
        }
        catch (Exception e)
        {
            var numberedException = new NumberedException(e);
            _logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
                nameof(DatabaseService), nameof(DeleteShoppingList));
            throw numberedException;
        }
    }*/

    public async Task<bool> DeleteItem(Guid itemId, SqlConnection sqlConnection)
    {
        var query = "DELETE FROM Item WHERE ItemID = @ItemID";

        List<SqlParameter> parameters =
        [
            new SqlParameter() { ParameterName = "@ItemID", Value = itemId, SqlDbType = SqlDbType.UniqueIdentifier }
        ];

        try
        {
            await using SqlCommand sqlCommand = new(query, sqlConnection);
            sqlCommand.Parameters.AddRange(parameters.ToArray());

            if (sqlConnection.State != ConnectionState.Open)
            {
                await sqlConnection.OpenAsync();
            }

            var result = await sqlCommand.ExecuteNonQueryAsync();

            return result > 0;
        }
        catch (NumberedException)
        {
            throw;
        }
        catch (Exception e)
        {
            var numberedException = new NumberedException(e);
            _logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
                nameof(DatabaseService), nameof(DeleteItem));
            throw numberedException;
        }
    }

    // nicht erwuenscht, weil die Datensaetze koennen in anderen Listen erwaehnt sein!
    /*public async Task<bool> DeleteUserRole(Guid roleId, SqlConnection sqlConnection)
    {
        try
        {
            var checkQuery = "SELECT COUNT(1) FROM ListMember WHERE UserRoleID = @UserRoleID";

            await using SqlCommand checkCommand = new(checkQuery, sqlConnection);
            checkCommand.Parameters.Add(new SqlParameter("@UserRoleID", roleId)
                { SqlDbType = SqlDbType.UniqueIdentifier });

            if (sqlConnection.State != ConnectionState.Open)
            {
                await sqlConnection.OpenAsync();
            }

            var count = Convert.ToInt32(await checkCommand.ExecuteScalarAsync());

            if (count > 0)
            {
                throw new NumberedException(
                    "Die Benutzerrolle wird von einem oder mehreren Benutzern verwendet und kann nicht gelöscht werden.");
            }

            var deleteQuery = "DELETE FROM UserRole WHERE UserRoleID = @UserRoleID";

            await using SqlCommand deleteCommand = new(deleteQuery, sqlConnection);
            deleteCommand.Parameters.Add(new SqlParameter("@UserRoleID", roleId)
                { SqlDbType = SqlDbType.UniqueIdentifier });

            var result = await deleteCommand.ExecuteNonQueryAsync();

            return result > 0;
        }
        catch (NumberedException)
        {
            throw;
        }
        catch (Exception e)
        {
            var numberedException = new NumberedException(e);
            _logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
                nameof(DatabaseService), nameof(DeleteUserRole));
            throw numberedException;
        }
    }*/

    // ToDo: add collaborator check!
    // List Admin cannot be removed from the list!
    public async Task<bool> RemoveCollaboraterFromShoppingList(Guid listId, Guid userId, SqlConnection sqlConnection)
    {
        var query = "DELETE FROM ListMember WHERE ShoppingListID = @ShoppingListID AND UserID = @UserID";

        List<SqlParameter> parameters =
        [
            new SqlParameter()
                { ParameterName = "@ShoppingListID", Value = listId, SqlDbType = SqlDbType.UniqueIdentifier },
            new SqlParameter() { ParameterName = "@UserID", Value = userId, SqlDbType = SqlDbType.UniqueIdentifier }
        ];

        try
        {
            await using SqlCommand sqlCommand = new(query, sqlConnection);
            sqlCommand.Parameters.AddRange(parameters.ToArray());

            if (sqlConnection.State != ConnectionState.Open)
            {
                await sqlConnection.OpenAsync();
            }

            var result = await sqlCommand.ExecuteNonQueryAsync();

            return result > 0;
        }
        catch (NumberedException)
        {
            throw;
        }
        catch (Exception e)
        {
            var numberedException = new NumberedException(e);
            _logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
                nameof(DatabaseService), nameof(RemoveCollaboraterFromShoppingList));
            throw numberedException;
        }
    }

    #endregion
}