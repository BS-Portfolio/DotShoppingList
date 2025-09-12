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
}