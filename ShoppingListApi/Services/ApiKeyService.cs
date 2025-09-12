using ShoppingListApi.Interfaces.Repositories;
using ShoppingListApi.Interfaces.Services;
using ShoppingListApi.Model.Entity;
using ShoppingListApi.Model.ReturnTypes;

namespace ShoppingListApi.Services;

public class ApiKeyService(IApiKeyRepository apiKeyRepository, ILogger<ApiKeyService> logger): IApiKeyService
{
    private readonly IApiKeyRepository _apiKeyRepository = apiKeyRepository;
    private readonly ILogger<ApiKeyService> _logger = logger;

    public IApiKeyRepository ApiKeyRepository => _apiKeyRepository;

    public async Task<AddRecordResult<ApiKey?, ApiKey?>> CreateAsync(Guid userId, CancellationToken ct = default)
    {
        try
        {
            var newKey = ApiKey.GenerateKey();
            var conflictingKey = await _apiKeyRepository.GetByKeyAsync(userId, newKey, ct);

            if (conflictingKey is not null) return new(false, null, true, conflictingKey);

            var (success, apiKey) = await _apiKeyRepository.CreateAsync(userId, newKey, ct);
            
            if (success is false || apiKey is null)
                return new(false, apiKey, false, null);

            return new(true, apiKey, false,null);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<RemoveRecordResult> DeleteExpiredAsync(Guid userId, Guid apiKeyId, CancellationToken ct = default)
    {
        try
        {
            var ids = await _apiKeyRepository.GetAllInvalidatedKeysBeforeDateAsync(ct);

            if (ids.Count == 0)
                return new(false, true, 0);

            return await _apiKeyRepository.DeleteBatchAsync(ids, ct);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}