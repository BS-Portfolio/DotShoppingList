namespace ShoppingListApi.Services;

public class ConnectionStringService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<ConnectionStringService> _logger;

    public ConnectionStringService(IServiceProvider serviceProvider)
    {
        _configuration = serviceProvider.GetRequiredService<IConfiguration>();
        _logger = serviceProvider.GetRequiredService<ILogger<ConnectionStringService>>();
    }

    public string GetConnectionString()
    {
        var user = "Milad";
        
        try
        {
            var connectionString = _configuration.GetConnectionString(user);

            if (connectionString is not null)
            {
                return connectionString;
            }
            else throw new KeyNotFoundException("Connection string not found");
        }
        catch (Exception e)
        {
            _logger.LogCritical(e, "Connection String could not b found for the following user: {user}", user);
            return "0";
        }
    }
}