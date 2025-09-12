using Microsoft.EntityFrameworkCore;
using ShoppingListApi.Data.Contexts;
using ShoppingListApi.Enums;
using ShoppingListApi.Interfaces.Repositories;
using ShoppingListApi.Model.Entity;
using ShoppingListApi.Model.ReturnTypes;

namespace ShoppingListApi.Repositories;

public class ApiKeyRepository(AppDbContext dbContext, ILogger<ApiKeyRepository> logger): IApiKeyRepository
{
    private readonly AppDbContext _dbContext = dbContext;
    private readonly ILogger<ApiKeyRepository> _logger = logger;

    public async Task<ApiKey?> GetWithDetailsByIdAsync(Guid userId, Guid apiKeyId, CancellationToken ct = default)
    {
        return await _dbContext.ApiKeys
            .Include(apiKey => apiKey.User).ThenInclude(user => user.EmailConfirmationTokens)
            .Include(apiKey => apiKey.User).ThenInclude(user => user.ListMemberships)
            .FirstOrDefaultAsync(key => key.ApiKeyId == apiKeyId && key.UserId == userId, ct);
    }

    public async Task<ApiKey?> GetWithoutDetailsByIdAsync(Guid userId, Guid apiKeyId, CancellationToken ct = default)
    {
        return await _dbContext.ApiKeys
            .FirstOrDefaultAsync(key => key.ApiKeyId == apiKeyId && key.UserId == userId, ct);
    }

    public async Task<List<Guid>> GetAllInvalidatedKeysBeforeDateAsync(CancellationToken ct = default)
    {
        return await _dbContext.ApiKeys
            .Where(ak => ak.IsValid == false || ak.ExpirationDateTime < DateTimeOffset.UtcNow)
            .Select(ak => ak.ApiKeyId)
            .ToListAsync(ct);
    }

    public async Task<List<ApiKey>> GetAllByUserId(Guid userId, ValidityCheck validityCheck,
        CancellationToken ct = default)
    {
        bool? validityCheckValue = validityCheck switch
        {
            ValidityCheck.IsValid => true,
            ValidityCheck.IsNotValid => false,
            _ => null
        };

        var query = _dbContext.ApiKeys.Where(ak => ak.UserId == userId);

        if (validityCheckValue.HasValue)
        {
            query = query.Where(ak => ak.IsValid == validityCheckValue.Value);
        }

        return await query.ToListAsync(ct);
    }

    public async Task<ApiKey?> GetByKeyAsync(Guid userId, string apiKey, CancellationToken ct = default)
    {
        return await _dbContext.ApiKeys.FirstOrDefaultAsync(ak => ak.Key == apiKey && ak.UserId == userId, ct);
    }

    public async Task<(bool Success, ApiKey? apiKey)> CreateAsync(Guid userId, string newKey, CancellationToken ct = default)
    {
        var key = ApiKey.GenerateKey();

        var apiKey = new ApiKey
        {
            ApiKeyId = Guid.NewGuid(),
            UserId = userId,
            Key = key,
            CreationDateTime = DateTimeOffset.UtcNow,
            ExpirationDateTime = DateTimeOffset.UtcNow.AddMinutes(45),
            IsValid = true
        };

        await _dbContext.ApiKeys.AddAsync(apiKey, ct);
        var checkResult = await _dbContext.SaveChangesAsync(ct);

        if (checkResult == 0)
        {
            _logger.LogWarning("Failed to create API key for user {UserId}", userId);
            return (false, null);
        }

        return (true, apiKey);
    }

    public async Task<bool> InvalidateAsync(Guid userId, Guid apiKeyId, CancellationToken ct = default)
    {
        var apiKey = await GetWithoutDetailsByIdAsync(userId, apiKeyId, ct);

        if (apiKey == null)
        {
            _logger.LogWarning("API key {ApiKeyId} not found for invalidation", apiKeyId);
            return false;
        }

        apiKey.IsValid = false;
        apiKey.ExpirationDateTime = DateTimeOffset.UtcNow;

        _dbContext.ApiKeys.Update(apiKey);

        var checkResult = await _dbContext.SaveChangesAsync(ct);

        if (checkResult == 0)
        {
            _logger.LogWarning("Failed to invalidate API key {ApiKeyId}", apiKeyId);
            return false;
        }

        return true;
    }

    public async Task<bool> InvalidateAllAsync(Guid userId, CancellationToken ct = default)
    {
        var apiKeys = await _dbContext.ApiKeys.Where(ak => ak.UserId == userId && ak.IsValid).ToListAsync(ct);

        if (!apiKeys.Any())
        {
            _logger.LogInformation("No valid API keys found for user {UserId} to invalidate", userId);
            return true;
        }

        foreach (var apiKey in apiKeys)
        {
            apiKey.IsValid = false;
            apiKey.ExpirationDateTime = DateTimeOffset.UtcNow;
        }

        _dbContext.ApiKeys.UpdateRange(apiKeys);
        var checkResult = await _dbContext.SaveChangesAsync(ct);

        if (checkResult == 0)
        {
            _logger.LogWarning("Failed to invalidate API keys for user {UserId}", userId);
            return false;
        }

        return true;
    }

    public async Task<RemoveRecordResult> DeleteAsync(Guid userId, Guid apiKeyId, CancellationToken ct = default)
    {
        var apiKey = await GetWithoutDetailsByIdAsync(userId, apiKeyId, ct);

        if (apiKey is null)
        {
            _logger.LogWarning("API key {ApiKeyId} not found for deletion", apiKeyId);
            return new(false, false, 0);
        }

        _dbContext.ApiKeys.Remove(apiKey);

        var checkResult = await _dbContext.SaveChangesAsync(ct);

        if (checkResult is not 1)
        {
            _logger.LogWarning("Failed to delete API key {ApiKeyId}", apiKeyId);
            return new RemoveRecordResult(true, false, checkResult);
        }

        return new RemoveRecordResult(true, true, 1);
    }
    public async Task<RemoveRecordResult> DeleteBatchAsync(List<Guid> apiKeyIds, CancellationToken ct = default)
    {
        var checkResult = await _dbContext.ApiKeys
            .Where(ak => apiKeyIds.Contains(ak.ApiKeyId))
            .ExecuteDeleteAsync(ct);

        if (checkResult != apiKeyIds.Count)
        {
            if (checkResult == 0)
                _logger.LogWarning("Failed to delete API keys in the provided list");
            else
                _logger.LogWarning("Only deleted {DeletedCount} out of {TotalCount} API keys in the provided list",
                    checkResult, apiKeyIds.Count);
            
            return new RemoveRecordResult(true, false, checkResult);
        }

        return new RemoveRecordResult(true, true, checkResult);
    }
}