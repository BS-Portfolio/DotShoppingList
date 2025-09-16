using ShoppingListApi.Interfaces.Repositories;
using ShoppingListApi.Model.Entity;
using ShoppingListApi.Model.ReturnTypes;

namespace ShoppingListApi.Interfaces.Services;

public interface IApiKeyService
{
    IApiKeyRepository ApiKeyRepository { get; }
    Task<AddRecordResult<ApiKey?, ApiKey?>> CreateAsync(Guid userId, CancellationToken ct = default);

    Task<UpdateRecordResult<ApiKey?>>
        FindAndInvalidateAsync(Guid userId, Guid apiKeyId, CancellationToken ct = default);

    Task<UpdateRecordResult<object?>>
        FindUserAndInvalidateAllByUserIdAsync(IListUserService listUserService, Guid userId,
            CancellationToken ct = default);

    Task<RemoveRecordResult> FindAndDeleteAsync(Guid userId, Guid apiKeyId, CancellationToken ct = default);
    Task<RemoveRecordResult> DeleteExpiredAsync(Guid userId, Guid apiKeyId, CancellationToken ct = default);
}