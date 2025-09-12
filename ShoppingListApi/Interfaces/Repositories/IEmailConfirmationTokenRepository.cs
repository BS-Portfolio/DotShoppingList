using ShoppingListApi.Enums;
using ShoppingListApi.Model.Entity;
using ShoppingListApi.Model.ReturnTypes;

namespace ShoppingListApi.Interfaces.Repositories;

public interface IEmailConfirmationTokenRepository
{
    Task<List<EmailConfirmationToken>> GetAllByUserIdAsync(Guid userId, ValidityCheck validityCheck,
        CancellationToken ct = default);

    Task<EmailConfirmationToken?> GetByIdWithUserDetails(Guid emailConfirmationTokenId, CancellationToken ct = default);
    Task<EmailConfirmationToken?> GetByTokenValue(Guid userId, string token, CancellationToken ct = default);
    Task<EmailConfirmationToken?> AddAsync(Guid userId, string generatedToken, CancellationToken ct = default);
    Task<UpdateRecordResult<object?>> MarkTokenAsUsedAsync(Guid userId, Guid emailConfirmationTokenId, CancellationToken ct = default);
    Task<bool> InvalidateAllByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<RemoveRecordResult> DeleteAsync(Guid userId, Guid emailConfirmationTokenId, CancellationToken ct = default);
    Task<RemoveRecordResult> DeleteBatchAsync(List<Guid> emailConfirmationTokenIds, CancellationToken ct = default);
}