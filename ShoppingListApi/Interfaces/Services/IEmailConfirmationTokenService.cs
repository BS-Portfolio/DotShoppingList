using ShoppingListApi.Interfaces.Repositories;
using ShoppingListApi.Model.Entity;
using ShoppingListApi.Model.ReturnTypes;

namespace ShoppingListApi.Interfaces.Services;

public interface IEmailConfirmationTokenService
{
    Task<AddRecordResult<EmailConfirmationToken?, EmailConfirmationToken?>> CheckConflictAndAdd(
        Guid userId, CancellationToken ct = default);

    Task<EmailConfirmationTokenValidationResult> CheckTokenValidity(Guid userId, string token,
        CancellationToken ct = default);

    Task<UpdateRecordResult<EmailConfirmationToken?>> FindUserAndInvalidateAllTokenByUserId(
        Guid userId, CancellationToken ct = default);

    Task<UpdateRecordResult<EmailConfirmationToken?>> FindUserAndInvalidateAllTokenByUserEmail(
        string userEmailAddress, CancellationToken ct = default);

    Task<UpdateRecordResult<EmailConfirmationToken?>> FindAndMarkTokenAsUsedById(Guid userId,
        Guid emailConfirmationTokenId, CancellationToken ct = default);

    Task<UpdateRecordResult<EmailConfirmationToken?>> FindAndMarkTokenAsUsedByTokenValue(Guid userId,
        string tokenValue, CancellationToken ct = default);

    Task<RemoveRecordResult> FindAndDeleteByTokenValueAsync(Guid userId, string token,
        CancellationToken ct = default);

    Task<RemoveRecordResult> FindAndDeleteByIdAsync(Guid userId, Guid emailConfirmationTokenId,
        CancellationToken ct = default);

    Task<RemoveRecordResult> DeleteAllUsedByUserIdAsync(Guid userId, CancellationToken ct = default);

    Task<RemoveRecordResult> DeleteAllUsedByUserEmailAsync(string userEmailAddress, CancellationToken ct = default);
}