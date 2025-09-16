using ShoppingListApi.Enums;
using ShoppingListApi.Interfaces.Repositories;
using ShoppingListApi.Interfaces.Services;
using ShoppingListApi.Model.Entity;
using ShoppingListApi.Model.ReturnTypes;

namespace ShoppingListApi.Services;

public class EmailConfirmationTokenService(
    IEmailConfirmationTokenRepository emailConfirmationTokenRepository,
    ILogger<EmailConfirmationTokenService> logger) : IEmailConfirmationTokenService
{
    private readonly IEmailConfirmationTokenRepository _emailConfirmationTokenRepository =
        emailConfirmationTokenRepository;

    private readonly ILogger<IEmailConfirmationTokenService> _logger = logger;

    public IEmailConfirmationTokenRepository EmailConfirmationTokenRepository => _emailConfirmationTokenRepository;

    public async Task<AddRecordResult<EmailConfirmationToken?, EmailConfirmationToken?>> CheckConflictAndAdd(
        Guid userId, CancellationToken ct = default)
    {
        try
        {
            var generatedToken = EmailConfirmationToken.GenerateToken();

            var conflictingToken = await _emailConfirmationTokenRepository.GetByTokenValue(userId, generatedToken, ct);

            if (conflictingToken is not null)
                return new(false, null, true, conflictingToken);

            var addedToken = await _emailConfirmationTokenRepository.AddAsync(userId, generatedToken, ct);

            if (addedToken is null)
                return new(false, null, false, null);

            return new(true, addedToken, false, null);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<EmailConfirmationTokenValidationResult> CheckTokenValidity(Guid userId, string token,
        CancellationToken ct = default)
    {
        try
        {
            var targetToken = await _emailConfirmationTokenRepository.GetByTokenValue(userId, token, ct);

            if (targetToken is null)
                return new(false, false);

            return new(true, EmailConfirmationToken.ValidateToken(targetToken));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<UpdateRecordResult<EmailConfirmationToken?>> FindUserAndInvalidateAllTokenByUserId(
        IListUserRepository listUserRepository, Guid userId,
        CancellationToken ct = default)
    {
        try
        {
            var targetUser =
                await listUserRepository.GetWithoutDetailsByIdAsync(userId, ct);

            if (targetUser is null)
                return new(false, false, false, null);

            var invalidateResult = await _emailConfirmationTokenRepository.InvalidateAllByUserIdAsync(userId, ct);

            if (invalidateResult is false)
                return new(true, false, false, null);

            return new(true, true, false, null);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<UpdateRecordResult<EmailConfirmationToken?>> FindUserAndInvalidateAllTokenByUserEmail(
        IListUserRepository listUserRepository, string userEmailAddress,
        CancellationToken ct = default)
    {
        try
        {
            var targetUser =
                await listUserRepository.GetWithoutDetailsByEmailAddressAsync(userEmailAddress, ct);

            if (targetUser is null)
                return new(false, false, false, null);

            var invalidateResult =
                await _emailConfirmationTokenRepository.InvalidateAllByUserIdAsync(targetUser.UserId, ct);

            if (invalidateResult is false)
                return new(true, false, false, null);

            return new(true, true, false, null);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<UpdateRecordResult<EmailConfirmationToken?>> FindAndMarkTokenAsUsedById(Guid userId,
        Guid emailConfirmationTokenId, CancellationToken ct = default)
    {
        try
        {
            var targetToken =
                await _emailConfirmationTokenRepository.GetByIdWithoutUserDetails(userId, emailConfirmationTokenId, ct);

            if (targetToken is null)
                return new(false, false, false, null);

            var markAsUsedResult = await _emailConfirmationTokenRepository.MarkTokenAsUsedAsync(targetToken, ct);

            if (markAsUsedResult is false)
                return new(true, false, false, null);

            return new(true, true, false, null);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<UpdateRecordResult<EmailConfirmationToken?>> FindAndMarkTokenAsUsedByTokenValue(Guid userId,
        string tokenValue, CancellationToken ct = default)
    {
        try
        {
            var targetToken = await _emailConfirmationTokenRepository.GetByTokenValue(userId, tokenValue, ct);

            if (targetToken is null)
                return new(false, false, false, null);

            var markAsUsedResult = await _emailConfirmationTokenRepository.MarkTokenAsUsedAsync(targetToken, ct);

            if (markAsUsedResult is false)
                return new(true, false, false, null);

            return new(true, true, false, null);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<RemoveRecordResult> FindAndDeleteByTokenValueAsync(Guid userId, string token,
        CancellationToken ct = default)
    {
        try
        {
            var targetToken = await _emailConfirmationTokenRepository.GetByTokenValue(userId, token, ct);
            if (targetToken is null)
                return new(false, false, 0);

            var deleteResult = await _emailConfirmationTokenRepository.DeleteAsync(targetToken, ct);
            if (deleteResult is false)
                return new(true, false, 0);

            return new(true, true, 1);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error deleting email confirmation token");
            throw;
        }
    }

    public async Task<RemoveRecordResult> FindAndDeleteByIdAsync(Guid userId, Guid emailConfirmationTokenId,
        CancellationToken ct = default)
    {
        try
        {
            var invalidateResult =
                await _emailConfirmationTokenRepository.GetByIdWithoutUserDetails(userId, emailConfirmationTokenId, ct);
            if (invalidateResult is null)
                return new(false, false, 0);

            var deleteResult = await _emailConfirmationTokenRepository.DeleteAsync(invalidateResult, ct);

            if (deleteResult is false)
                return new(true, false, 0);

            return new(true, true, 1);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error invalidating email confirmation tokens for user {UserId}", userId);
            throw;
        }
    }

    public async Task<RemoveRecordResult> DeleteAllUsedByUserIdAsync(IListUserRepository listUserRepository,
        Guid userId, CancellationToken ct = default)
    {
        try
        {
            var targetUser = await listUserRepository.GetWithoutDetailsByIdAsync(userId, ct);

            if (targetUser is null)
                return new(false, false, 0);

            var emailConfirmationTokenIdsToBeRemoved = (await
                    _emailConfirmationTokenRepository.GetAllByUserIdAsync(userId, ValidityCheck.IsNotValid, ct))
                .Select(t => t.EmailConfirmationTokenId).ToList();

            if (emailConfirmationTokenIdsToBeRemoved.Count == 0)
                return new(true, true, 0);

            return await _emailConfirmationTokenRepository.DeleteBatchAsync(emailConfirmationTokenIdsToBeRemoved, ct);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<RemoveRecordResult> DeleteAllUsedByUserEmailAsync(IListUserRepository listUserRepository,
        string userEmailAddress, CancellationToken ct = default)
    {
        try
        {
            var targetUser = await listUserRepository.GetWithoutDetailsByEmailAddressAsync(userEmailAddress, ct);

            if (targetUser is null)
                return new(false, false, 0);

            var emailConfirmationTokenIdsToBeRemoved = (await
                    _emailConfirmationTokenRepository.GetAllByUserIdAsync(targetUser.UserId, ValidityCheck.IsNotValid,
                        ct))
                .Select(t => t.EmailConfirmationTokenId).ToList();

            if (emailConfirmationTokenIdsToBeRemoved.Count == 0)
                return new(true, true, 0);

            return await _emailConfirmationTokenRepository.DeleteBatchAsync(emailConfirmationTokenIdsToBeRemoved, ct);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}