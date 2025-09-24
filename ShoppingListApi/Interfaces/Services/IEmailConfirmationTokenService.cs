using ShoppingListApi.Interfaces.Repositories;
using ShoppingListApi.Model.Entity;
using ShoppingListApi.Model.ReturnTypes;

namespace ShoppingListApi.Interfaces.Services;

public interface IEmailConfirmationTokenService
{
    /// <summary>
    /// Adds a new email confirmation token for a user, checking for conflicts with existing tokens.
    /// Returns the result including the added token or a conflicting token if one exists.
    /// </summary>
    Task<AddRecordResult<EmailConfirmationToken?, EmailConfirmationToken?>> CheckConflictAndAdd(
        Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Checks the validity of an email confirmation token for a user.
    /// Returns a validation result indicating existence and validity.
    /// </summary>
    Task<EmailConfirmationTokenValidationResult> CheckTokenValidity(Guid userId, string token,
        CancellationToken ct = default);

    /// <summary>
    /// Finds a user by ID and invalidates all their email confirmation tokens.
    /// Returns the result including success and state flags.
    /// </summary>
    Task<UpdateRecordResult<EmailConfirmationToken?>> FindUserAndInvalidateAllTokenByUserId(
        Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Finds a user by email and invalidates all their email confirmation tokens.
    /// Returns the result including success and state flags.
    /// </summary>
    Task<UpdateRecordResult<EmailConfirmationToken?>> FindUserAndInvalidateAllTokenByUserEmail(
        string userEmailAddress, CancellationToken ct = default);

    /// <summary>
    /// Finds and marks an email confirmation token as used by its ID for a user.
    /// Returns the result including success and state flags.
    /// </summary>
    Task<UpdateRecordResult<EmailConfirmationToken?>> FindAndMarkTokenAsUsedById(Guid userId,
        Guid emailConfirmationTokenId, CancellationToken ct = default);

    /// <summary>
    /// Finds and marks an email confirmation token as used by its token value for a user.
    /// Returns the result including success and state flags.
    /// </summary>
    Task<UpdateRecordResult<EmailConfirmationToken?>> FindAndMarkTokenAsUsedByTokenValue(Guid userId,
        string tokenValue, CancellationToken ct = default);

    /// <summary>
    /// Finds and deletes an email confirmation token by its token value for a user.
    /// Returns the result including success and affected records count.
    /// </summary>
    Task<RemoveRecordResult> FindAndDeleteByTokenValueAsync(Guid userId, string token,
        CancellationToken ct = default);

    /// <summary>
    /// Finds and deletes an email confirmation token by its ID for a user.
    /// Returns the result including success and affected records count.
    /// </summary>
    Task<RemoveRecordResult> FindAndDeleteByIdAsync(Guid userId, Guid emailConfirmationTokenId,
        CancellationToken ct = default);

    /// <summary>
    /// Deletes all used or expired email confirmation tokens for a user by their ID.
    /// Returns the result including success and affected records count.
    /// </summary>
    Task<RemoveRecordResult> DeleteAllUsedByUserIdAsync(Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Deletes all used or expired email confirmation tokens for a user by their email address.
    /// Returns the result including success and affected records count.
    /// </summary>
    Task<RemoveRecordResult> DeleteAllUsedByUserEmailAsync(string userEmailAddress, CancellationToken ct = default);
}