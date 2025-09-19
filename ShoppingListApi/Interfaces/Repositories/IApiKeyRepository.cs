using ShoppingListApi.Enums;
using ShoppingListApi.Model.Entity;

namespace ShoppingListApi.Interfaces.Repositories;

public interface IApiKeyRepository
{
    Task<ApiKey?> GetWithDetailsByIdAsync(Guid userId, Guid apiKeyId, CancellationToken ct = default);
    Task<ApiKey?> GetWithoutDetailsByIdAsync(Guid userId, Guid apiKeyId, CancellationToken ct = default);
    Task<List<ApiKey>> GetAllInvalidatedKeysBeforeDateAsync(CancellationToken ct = default);
    Task<List<ApiKey>> GetAllByUserIdAsync(Guid userId, ValidityCheck validityCheck, CancellationToken ct = default);
    Task<ApiKey?> GetByKeyAsync(Guid userId, string apiKey, CancellationToken ct = default);
    Task<ApiKey> CreateAsync(Guid userId, CancellationToken ct = default);
    void Invalidate(ApiKey targetApiKey);
    Task<int> InvalidateAllByUserIdAsync(Guid userId, CancellationToken ct = default);
    void Delete(ApiKey targetApiKey);
    void DeleteBatch(List<ApiKey> apiKeys);
}