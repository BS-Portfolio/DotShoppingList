using ShoppingListApi.Configs;
using ShoppingListApi.Controllers;
using ShoppingListApi.Exceptions;
using ShoppingListApi.Model.ReturnTypes;

namespace ShoppingListApi.Services;

public class MyAuthenticationService
{
    private readonly string _connectionString;
    private readonly ConnectionStringService _connectionStringService;
    private readonly ILogger<ShoppingListApiController> _logger;

    public MyAuthenticationService(IServiceProvider serviceProvider)
    {
        _connectionStringService = serviceProvider.GetRequiredService<ConnectionStringService>();
        _connectionString = _connectionStringService.GetConnectionString();
        _logger = serviceProvider.GetRequiredService<ILogger<ShoppingListApiController>>();
    }
    
        public async Task<AuthenticationReturn> AuthenticateAsync(Guid userId, string apiKey)
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
                nameof(DatabaseService), nameof(AuthenticateAsync));
            throw numberedException;
        }
        finally
        {
            await sqlConnection.CloseAsync();
        }
    }
};