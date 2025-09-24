using ShoppingListApi.Model.Entity;
using ShoppingListApi.Model.ReturnTypes;

namespace ShoppingListApi.Interfaces.Services;

public interface IApiKeyService
{
    /// <summary>
    /// Retrieves an ApiKey for a user by its ID, without including related entities.
    /// Returns null if not found.
    /// </summary>
    Task<ApiKey?> GetWithoutDetailsByIdAsync(Guid userId, Guid apiKeyId, CancellationToken ct = default);

    /// <summary>
    /// Creates a new ApiKey for a user. Returns the result including the created key or a conflicting key if one exists.
    /// </summary>
    Task<AddRecordResult<ApiKey?, ApiKey?>> CreateAsync(Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Finds an ApiKey for a user by its ID and invalidates it if valid. Returns the result including success and state flags.
    /// </summary>
    Task<UpdateRecordResult<ApiKey?>> FindAndInvalidateAsync(
        Guid userId, Guid apiKeyId, CancellationToken ct = default);

    /// <summary>
    /// Finds a user by ID and invalidates all their ApiKeys. Returns the result including success and state flags.
    /// </summary>
    Task<UpdateRecordResult<object?>> FindUserAndInvalidateAllByUserIdAsync(Guid userId,
        CancellationToken ct = default);

    /// <summary>
    /// Finds and deletes an ApiKey for a user by its ID. Returns the result including success and affected records count.
    /// </summary>
    Task<RemoveRecordResult> FindAndDeleteAsync(Guid userId, Guid apiKeyId, CancellationToken ct = default);

    /// <summary>
    /// Deletes all expired or invalidated ApiKeys. Returns the result including success and affected records count.
    /// </summary>
    Task<RemoveRecordResult> DeleteExpiredAsync(CancellationToken ct = default);
}