using ShoppingListApi.Model.Entity;
using ShoppingListApi.Model.ReturnTypes;

namespace ShoppingListApi.Interfaces.Services;

public interface IApiKeyService
{
    Task<ApiKey?> GetWithoutDetailsByIdAsync(Guid userId, Guid apiKeyId, CancellationToken ct = default);
    Task<AddRecordResult<ApiKey?, ApiKey?>> CreateAsync(Guid userId, CancellationToken ct = default);

    Task<UpdateRecordResult<ApiKey?>> FindAndInvalidateAsync(
        Guid userId, Guid apiKeyId, CancellationToken ct = default);

    Task<UpdateRecordResult<object?>> FindUserAndInvalidateAllByUserIdAsync(Guid userId,
        CancellationToken ct = default);

    Task<RemoveRecordResult> FindAndDeleteAsync(Guid userId, Guid apiKeyId, CancellationToken ct = default);
    Task<RemoveRecordResult> DeleteExpiredAsync(CancellationToken ct = default);
}