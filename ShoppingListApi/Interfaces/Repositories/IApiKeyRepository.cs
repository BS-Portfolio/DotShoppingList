using ShoppingListApi.Enums;
using ShoppingListApi.Model.Entity;
using ShoppingListApi.Model.ReturnTypes;

namespace ShoppingListApi.Interfaces.Repositories;

public interface IApiKeyRepository
{
    Task<ApiKey?> GetWithDetailsByIdAsync(Guid userId, Guid apiKeyId, CancellationToken ct = default);
    Task<ApiKey?> GetWithoutDetailsByIdAsync(Guid userId, Guid apiKeyId, CancellationToken ct = default);
    Task<List<Guid>> GetAllInvalidatedKeysBeforeDateAsync(CancellationToken ct = default);
    Task<List<ApiKey>> GetAllByUserId(Guid userId, ValidityCheck validityCheck, CancellationToken ct = default);
    Task<ApiKey?> GetByKeyAsync(Guid userId, string apiKey, CancellationToken ct = default);
    Task<(bool Success, ApiKey? apiKey)> CreateAsync(Guid userId, string newKey, CancellationToken ct = default);
    Task<bool> InvalidateAsync(ApiKey targetApiKey, CancellationToken ct = default);
    Task<bool> InvalidateAllByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<RemoveRecordResult> DeleteAsync(ApiKey targetApiKey, CancellationToken ct = default);
    Task<RemoveRecordResult> DeleteBatchAsync(List<Guid> apiKeyIds, CancellationToken ct = default);
}