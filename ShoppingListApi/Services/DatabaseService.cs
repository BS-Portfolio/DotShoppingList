using System.Data;
using ShoppingListApi.Configs;
using ShoppingListApi.Exceptions;
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

    public async Task<List<UserRole>> GetUserRoles(SqlConnection sqlConnection)
    {
        List<UserRole> userRoles = [];

        string query = "SELECT UserRoleID, UserRoleTitle, EnumIndex FROM UserRole";

        using SqlCommand sqlCommand = new(query, sqlConnection);

        try
        {
            if (sqlConnection.State != ConnectionState.Open)
            {
                await sqlConnection.OpenAsync();
            }

            using SqlDataReader sqlReader = await sqlCommand.ExecuteReaderAsync();

            if (sqlReader.HasRows)
            {
                while (await sqlReader.ReadAsync())
                {
                    userRoles.Add(new UserRole(sqlReader.GetGuid(0), sqlReader.GetString(1), sqlReader.GetInt32(2)));
                }
            }

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

    public async Task<(bool success, Guid? id)> AddUserRole(SqlConnection sqlConnection, UserRolePost userRolePost)
    {
        const bool success = true;

        string query =
            "INSERT INTO UserRole (UserRoleID, UserRoleTitle, EnumIndex) VALUES (@UserRoleID, @UserRoleTitle, @EnumIndex)";

        using SqlCommand sqlCommand = new(query, sqlConnection);

        Guid userRoleId = Guid.NewGuid();

        sqlCommand.Parameters.AddRange([
            new SqlParameter()
                { ParameterName = "@UserRoleID", Value = userRoleId, SqlDbType = SqlDbType.UniqueIdentifier },
            new SqlParameter() { ParameterName = "@UserRoleTitle", Value = userRolePost.UserRoleTitle },
            new SqlParameter() { ParameterName = "@EnumIndex", Value = (int)userRolePost.UserRoleEnum }
        ]);

        try
        {
            if (sqlConnection.State != ConnectionState.Open)
            {
                await sqlConnection.OpenAsync();
            }

            int checkResult = await sqlCommand.ExecuteNonQueryAsync();

            if (checkResult == 1)
            {
                return (success, userRoleId);
            }

            return (!success, null);
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
}