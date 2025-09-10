using ShoppingListApi.Data.Contexts;

namespace ShoppingListApi.Repositories;

public class ApiKeyRepository(AppDbContext dbContext, ILogger<ApiKeyRepository> logger)
{
    private readonly AppDbContext _dbContext = dbContext;
    private readonly ILogger<ApiKeyRepository> _logger = logger;
    

}