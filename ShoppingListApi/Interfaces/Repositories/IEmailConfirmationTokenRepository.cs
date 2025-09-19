using ShoppingListApi.Enums;
using ShoppingListApi.Model.Entity;

namespace ShoppingListApi.Interfaces.Repositories;

public interface IEmailConfirmationTokenRepository
{
    Task<List<EmailConfirmationToken>> GetAllByUserIdAsync(Guid userId, ValidityCheck validityCheck,
        CancellationToken ct = default);

    Task<EmailConfirmationToken?> GetByIdWithUserDetails(Guid userId, Guid emailConfirmationTokenId,
        CancellationToken ct = default);

    Task<EmailConfirmationToken?> GetByIdWithoutUserDetails(Guid userId, Guid emailConfirmationTokenId,
        CancellationToken ct = default);

    Task<EmailConfirmationToken?> GetByTokenValue(Guid userId, string token, CancellationToken ct = default);
    EmailConfirmationToken Add(Guid userId, string generatedToken);

    void MarkTokenAsUsed(EmailConfirmationToken targetEmailConfirmationToken);

    Task<int> InvalidateAllByUserIdAsync(Guid userId, CancellationToken ct = default);
    void Delete(EmailConfirmationToken targetEmailConfirmationToken);
    void DeleteBatch(List<EmailConfirmationToken> emailConfirmationTokens);
}