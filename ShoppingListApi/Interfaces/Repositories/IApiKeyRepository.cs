using ShoppingListApi.Enums;
using ShoppingListApi.Model.Entity;

namespace ShoppingListApi.Interfaces.Repositories;

public interface IApiKeyRepository
{
    /// <summary>
    /// Retrieves an ApiKey with related User, EmailConfirmationTokens, and ListMemberships by user and API key ID.
    /// Returns null if not found.
    /// </summary>
    Task<ApiKey?> GetWithDetailsByIdAsync(Guid userId, Guid apiKeyId, CancellationToken ct = default);

    /// <summary>
    /// Retrieves an ApiKey by user and API key ID without including related entities.
    /// Returns null if not found.
    /// </summary>
    Task<ApiKey?> GetWithoutDetailsByIdAsync(Guid userId, Guid apiKeyId, CancellationToken ct = default);

    /// <summary>
    /// Gets all ApiKeys that are invalidated or expired before the current UTC date/time.
    /// </summary>
    Task<List<ApiKey>> GetAllInvalidatedKeysBeforeDateAsync(CancellationToken ct = default);

    /// <summary>
    /// Retrieves all ApiKeys for a user, optionally filtered by validity (valid, not valid, or all).
    /// </summary>
    Task<List<ApiKey>> GetAllByUserIdAsync(Guid userId, ValidityCheck validityCheck, CancellationToken ct = default);

    /// <summary>
    /// Retrieves an ApiKey for a user by the API key string value.
    /// Returns null if not found.
    /// </summary>
    Task<ApiKey?> GetByKeyForUserAsync(Guid userId, string apiKey, CancellationToken ct = default);

    /// <summary>
    /// Retrieves an ApiKey by the API key string value, regardless of user.
    /// Returns null if not found.
    /// </summary>
    Task<ApiKey?> GetByKeyAsync(string apiKey, CancellationToken ct = default);

    /// <summary>
    /// Creates a new ApiKey for a user with a 3-hour expiration and marks it as valid.
    /// Does not save changes to the database.
    /// </summary>
    Task<ApiKey> CreateAsync(Guid userId, string newKey, CancellationToken ct = default);

    /// <summary>
    /// Invalidates the specified ApiKey and sets its expiration to the current UTC date/time.
    /// Updates the entity in the database context but does not save changes.
    /// </summary>
    void Invalidate(ApiKey targetApiKey);

    /// <summary>
    /// Invalidates all valid ApiKeys for a user and sets their expiration to the current UTC date/time.
    /// Returns the number of keys invalidated. Does not save changes.
    /// </summary>
    Task<int> InvalidateAllByUserIdAsync(Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Removes the specified ApiKey from the database context. Does not save changes.
    /// </summary>
    void Delete(ApiKey targetApiKey);

    /// <summary>
    /// Removes a batch of ApiKeys from the database context. Does not save changes.
    /// </summary>
    void DeleteBatch(List<ApiKey> apiKeys);
}