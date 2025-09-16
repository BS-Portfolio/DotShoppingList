using ShoppingListApi.Interfaces.Repositories;
using ShoppingListApi.Interfaces.Services;
using ShoppingListApi.Model.Entity;
using ShoppingListApi.Model.ReturnTypes;

namespace ShoppingListApi.Services;

public class ApiKeyService(IApiKeyRepository apiKeyRepository, ILogger<ApiKeyService> logger) : IApiKeyService
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

            return new(true, apiKey, false, null);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<UpdateRecordResult<ApiKey?>> FindAndInvalidateAsync(Guid userId, Guid apiKeyId,
        CancellationToken ct = default)
    {
        try
        {
            var targetApiKey = await _apiKeyRepository.GetWithoutDetailsByIdAsync(userId, apiKeyId, ct);

            if (targetApiKey is null)
                return new(false, false, true, null);

            if (targetApiKey.IsValid is false)
                return new(true, true, false, null);

            var success = await _apiKeyRepository.InvalidateAsync(targetApiKey, ct);

            if (success is false)
                return new(true, false, false, null);

            return new(true, true, false, targetApiKey);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<UpdateRecordResult<object?>> FindUserAndInvalidateAllByUserIdAsync(
        IListUserService listUserService, Guid userId,
        CancellationToken ct = default)
    {
        try
        {
            var targetUser = await listUserService.ListUserRepository.GetWithoutDetailsByIdAsync(userId, ct);

            if (targetUser is null) return new(false, false, false, null);

            var success = await _apiKeyRepository.InvalidateAllByUserIdAsync(userId, ct);

            if (success is false) return new(true, false, false, null);

            return new(true, true, false, null);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<RemoveRecordResult> FindAndDeleteAsync(Guid userId, Guid apiKeyId, CancellationToken ct = default)
    {
        try
        {
            var targetApiKey = await _apiKeyRepository.GetWithoutDetailsByIdAsync(userId, apiKeyId, ct);

            if (targetApiKey is null) return new(false, false, 0);
            
            return await _apiKeyRepository.DeleteAsync(targetApiKey, ct);
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