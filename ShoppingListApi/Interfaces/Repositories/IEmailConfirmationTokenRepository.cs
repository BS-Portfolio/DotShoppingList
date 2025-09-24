using ShoppingListApi.Enums;
using ShoppingListApi.Model.Entity;

namespace ShoppingListApi.Interfaces.Repositories;

public interface IEmailConfirmationTokenRepository
{
    /// <summary>
    /// Retrieves all email confirmation tokens for a user, filtered by validity (valid, not valid, or all).
    /// Valid tokens are unused and not expired; invalid tokens are used or expired.
    /// </summary>
    Task<List<EmailConfirmationToken>> GetAllByUserIdAsync(Guid userId, ValidityCheck validityCheck,
        CancellationToken ct = default);

    /// <summary>
    /// Retrieves an email confirmation token by user and token ID, including user details.
    /// Returns null if not found.
    /// </summary>
    Task<EmailConfirmationToken?> GetByIdWithUserDetails(Guid userId, Guid emailConfirmationTokenId,
        CancellationToken ct = default);

    /// <summary>
    /// Retrieves an email confirmation token by user and token ID, without including user details.
    /// Returns null if not found.
    /// </summary>
    Task<EmailConfirmationToken?> GetByIdWithoutUserDetails(Guid userId, Guid emailConfirmationTokenId,
        CancellationToken ct = default);

    /// <summary>
    /// Retrieves an email confirmation token for a user by the token string value.
    /// Returns null if not found.
    /// </summary>
    Task<EmailConfirmationToken?> GetByTokenValue(Guid userId, string token, CancellationToken ct = default);

    /// <summary>
    /// Adds a new email confirmation token for a user, valid for 24 hours and marked as unused.
    /// Does not save changes to the database.
    /// </summary>
    EmailConfirmationToken Add(Guid userId, string generatedToken);

    /// <summary>
    /// Marks the specified email confirmation token as used and sets its expiration to now if not already expired.
    /// </summary>
    void MarkTokenAsUsed(EmailConfirmationToken targetEmailConfirmationToken);

    /// <summary>
    /// Invalidates all unused or unexpired email confirmation tokens for a user by marking them as used and setting expiration to now.
    /// Returns the number of tokens invalidated.
    /// </summary>
    Task<int> InvalidateAllByUserIdAsync(Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Removes the specified email confirmation token from the database context. Does not save changes.
    /// </summary>
    void Delete(EmailConfirmationToken targetEmailConfirmationToken);

    /// <summary>
    /// Removes a batch of email confirmation tokens from the database context. Does not save changes.
    /// </summary>
    void DeleteBatch(List<EmailConfirmationToken> emailConfirmationTokens);
}