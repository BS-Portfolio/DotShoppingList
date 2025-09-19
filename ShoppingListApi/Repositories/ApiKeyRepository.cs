using Microsoft.EntityFrameworkCore;
using ShoppingListApi.Data.Contexts;
using ShoppingListApi.Enums;
using ShoppingListApi.Interfaces.Repositories;
using ShoppingListApi.Model.Entity;

namespace ShoppingListApi.Repositories;

public class ApiKeyRepository(AppDbContext dbContext) : IApiKeyRepository
{
    private readonly AppDbContext _dbContext = dbContext;

    public async Task<ApiKey?> GetWithDetailsByIdAsync(Guid userId, Guid apiKeyId, CancellationToken ct = default)
    {
        return await _dbContext.ApiKeys
            .Include(apiKey => apiKey.User).ThenInclude(user => user!.EmailConfirmationTokens)
            .Include(apiKey => apiKey.User).ThenInclude(user => user!.ListMemberships)
            .FirstOrDefaultAsync(key => key.ApiKeyId == apiKeyId && key.UserId == userId, ct);
    }

    public async Task<ApiKey?> GetWithoutDetailsByIdAsync(Guid userId, Guid apiKeyId, CancellationToken ct = default)
    {
        return await _dbContext.ApiKeys
            .FirstOrDefaultAsync(key => key.ApiKeyId == apiKeyId && key.UserId == userId, ct);
    }

    public async Task<List<ApiKey>> GetAllInvalidatedKeysBeforeDateAsync(CancellationToken ct = default)
    {
        return await _dbContext.ApiKeys
            .Where(ak => ak.IsValid == false || ak.ExpirationDateTime < DateTimeOffset.UtcNow)
            .ToListAsync(ct);
    }

    public async Task<List<ApiKey>> GetAllByUserIdAsync(Guid userId, ValidityCheck validityCheck,
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

    public async Task<ApiKey> CreateAsync(Guid userId, string newKey,
        CancellationToken ct = default)
    {

        var apiKey = new ApiKey
        {
            ApiKeyId = Guid.NewGuid(),
            UserId = userId,
            Key = newKey,
            CreationDateTime = DateTimeOffset.UtcNow,
            ExpirationDateTime = DateTimeOffset.UtcNow.AddMinutes(45),
            IsValid = true
        };

        await _dbContext.ApiKeys.AddAsync(apiKey, ct);

        return apiKey;
    }

    public void Invalidate(ApiKey targetApiKey)
    {
        targetApiKey.IsValid = false;
        targetApiKey.ExpirationDateTime = DateTimeOffset.UtcNow;

        _dbContext.ApiKeys.Update(targetApiKey);
    }

    public async Task<int> InvalidateAllByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        var apiKeys = await _dbContext.ApiKeys
            .Where(ak => ak.UserId == userId && ak.IsValid).ToListAsync(ct);

        if (apiKeys.Count == 0)
        {
            return 0;
        }

        foreach (var apiKey in apiKeys)
        {
            apiKey.IsValid = false;
            apiKey.ExpirationDateTime = DateTimeOffset.UtcNow;
        }

        return apiKeys.Count;
    }

    public void Delete(ApiKey targetApiKey)
    {
        _dbContext.ApiKeys.Remove(targetApiKey);
    }

    public void DeleteBatch(List<ApiKey> apiKeys)
    {
        _dbContext.ApiKeys.RemoveRange(apiKeys);
    }
}