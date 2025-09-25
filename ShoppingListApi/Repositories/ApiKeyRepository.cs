using Microsoft.EntityFrameworkCore;
using ShoppingListApi.Data.Contexts;
using ShoppingListApi.Enums;
using ShoppingListApi.Interfaces.Repositories;
using ShoppingListApi.Model.Entity;

namespace ShoppingListApi.Repositories;

public class ApiKeyRepository(AppDbContext dbContext) : IApiKeyRepository
{
    private readonly AppDbContext _dbContext = dbContext;

    /// <summary>
    /// Retrieves an ApiKey with related User, EmailConfirmationTokens, and ListMemberships by user and API key ID.
    /// Returns null if not found.
    /// </summary>
    public async Task<ApiKey?> GetWithDetailsByIdAsync(Guid userId, Guid apiKeyId, CancellationToken ct = default)
    {
        return await _dbContext.ApiKeys
            .Include(apiKey => apiKey.User).ThenInclude(user => user!.EmailConfirmationTokens)
            .Include(apiKey => apiKey.User).ThenInclude(user => user!.ListMemberships)
            .FirstOrDefaultAsync(key => key.ApiKeyId == apiKeyId && key.UserId == userId, ct);
    }

    /// <summary>
    /// Retrieves an ApiKey by user and API key ID without including related entities.
    /// Returns null if not found.
    /// </summary>
    public async Task<ApiKey?> GetWithoutDetailsByIdAsync(Guid userId, Guid apiKeyId, CancellationToken ct = default)
    {
        return await _dbContext.ApiKeys
            .FirstOrDefaultAsync(key => key.ApiKeyId == apiKeyId && key.UserId == userId, ct);
    }

    /// <summary>
    /// Gets all ApiKeys that are invalidated or expired before the current UTC date/time.
    /// </summary>
    public async Task<List<ApiKey>> GetAllInvalidatedKeysBeforeDateAsync(CancellationToken ct = default)
    {
        return await _dbContext.ApiKeys
            .Where(ak => ak.IsValid == false || ak.ExpirationDateTime < DateTimeOffset.UtcNow)
            .ToListAsync(ct);
    }

    /// <summary>
    /// Retrieves all ApiKeys for a user, optionally filtered by validity (valid, not valid, or all).
    /// </summary>
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

    /// <summary>
    /// Retrieves an ApiKey for a user by the API key string value.
    /// Returns null if not found.
    /// </summary>
    public async Task<ApiKey?> GetByKeyForUserAsync(Guid userId, string apiKey, CancellationToken ct = default)
    {
        return await _dbContext.ApiKeys.FirstOrDefaultAsync(ak => ak.Key == apiKey && ak.UserId == userId, ct);
    }
    
    /// <summary>
    /// Retrieves an ApiKey by the API key string value, regardless of user.
    /// Returns null if not found.
    /// </summary>
    public async Task<ApiKey?> GetByKeyAsync(string apiKey, CancellationToken ct = default)
    {
        return await _dbContext.ApiKeys.FirstOrDefaultAsync(ak => ak.Key == apiKey, ct);
    }

    /// <summary>
    /// Creates a new ApiKey for a user with a 3-hour expiration and marks it as valid.
    /// Does not save changes to the database.
    /// </summary>
    public async Task<ApiKey> CreateAsync(Guid userId, string newKey,
        CancellationToken ct = default)
    {

        var apiKey = new ApiKey
        {
            ApiKeyId = Guid.NewGuid(),
            UserId = userId,
            Key = newKey,
            CreationDateTime = DateTimeOffset.UtcNow,
            ExpirationDateTime = DateTimeOffset.UtcNow.AddHours(3),
            IsValid = true
        };

        await _dbContext.ApiKeys.AddAsync(apiKey, ct);

        return apiKey;
    }

    /// <summary>
    /// Invalidates the specified ApiKey and sets its expiration to the current UTC date/time.
    /// Updates the entity in the database context but does not save changes.
    /// </summary>
    public void Invalidate(ApiKey targetApiKey)
    {
        targetApiKey.IsValid = false;
        targetApiKey.ExpirationDateTime = DateTimeOffset.UtcNow;

        _dbContext.ApiKeys.Update(targetApiKey);
    }

    /// <summary>
    /// Invalidates all valid ApiKeys for a user and sets their expiration to the current UTC date/time.
    /// Returns the number of keys invalidated. Does not save changes.
    /// </summary>
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

    /// <summary>
    /// Removes the specified ApiKey from the database context. Does not save changes.
    /// </summary>
    public void Delete(ApiKey targetApiKey)
    {
        _dbContext.ApiKeys.Remove(targetApiKey);
    }

    /// <summary>
    /// Removes a batch of ApiKeys from the database context. Does not save changes.
    /// </summary>
    public void DeleteBatch(List<ApiKey> apiKeys)
    {
        _dbContext.ApiKeys.RemoveRange(apiKeys);
    }
}