using ShoppingListApi.Interfaces.Repositories;
using ShoppingListApi.Model.Entity;
using ShoppingListApi.Model.ReturnTypes;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ShoppingListApi.Interfaces.Services;

public interface IEmailConfirmationTokenService
{
    IEmailConfirmationTokenRepository EmailConfirmationTokenRepository { get; }

    Task<AddRecordResult<EmailConfirmationToken?, EmailConfirmationToken?>> CheckConflictAndAdd(
        Guid userId, CancellationToken ct = default);

    Task<EmailConfirmationTokenValidationResult> CheckTokenValidity(Guid userId, string token,
        CancellationToken ct = default);

    Task<UpdateRecordResult<EmailConfirmationToken?>> FindUserAndInvalidateAllTokenByUserId(
        IListUserRepository listUserRepository, Guid userId,
        CancellationToken ct = default);

    Task<UpdateRecordResult<EmailConfirmationToken?>> FindUserAndInvalidateAllTokenByUserEmail(
        IListUserRepository listUserRepository, string userEmailAddress,
        CancellationToken ct = default);

    Task<UpdateRecordResult<EmailConfirmationToken?>> FindAndMarkTokenAsUsedById(Guid userId,
        Guid emailConfirmationTokenId, CancellationToken ct = default);

    Task<UpdateRecordResult<EmailConfirmationToken?>> FindAndMarkTokenAsUsedByTokenValue(Guid userId,
        string tokenValue, CancellationToken ct = default);

    Task<RemoveRecordResult> FindAndDeleteByTokenValueAsync(Guid userId, string token,
        CancellationToken ct = default);

    Task<RemoveRecordResult> FindAndDeleteByIdAsync(Guid userId, Guid emailConfirmationTokenId,
        CancellationToken ct = default);

    Task<RemoveRecordResult> DeleteAllUsedByUserIdAsync(IListUserRepository listUserRepository,
        Guid userId, CancellationToken ct = default);

    Task<RemoveRecordResult> DeleteAllUsedByUserEmailAsync(IListUserRepository listUserRepository,
        string userEmailAddress, CancellationToken ct = default);
}