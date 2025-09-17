using ShoppingListApi.Configs;
using ShoppingListApi.Enums;
using ShoppingListApi.Exceptions;
using ShoppingListApi.Interfaces.Repositories;
using ShoppingListApi.Interfaces.Services;
using ShoppingListApi.Model.Entity;
using ShoppingListApi.Model.ReturnTypes;

namespace ShoppingListApi.Services;

public class EmailConfirmationTokenService(
    IUnitOfWork unitOfWork,
    ILogger<EmailConfirmationTokenService> logger) : IEmailConfirmationTokenService
{
    private readonly IUnitOfWork _unitOfWork =
        unitOfWork;

    private readonly ILogger<IEmailConfirmationTokenService> _logger = logger;

    public async Task<AddRecordResult<EmailConfirmationToken?, EmailConfirmationToken?>> CheckConflictAndAdd(
        Guid userId, CancellationToken ct = default)
    {
        try
        {
            var generatedToken = EmailConfirmationToken.GenerateToken();

            var conflictingToken =
                await _unitOfWork.EmailConfirmationTokenRepository.GetByTokenValue(userId, generatedToken, ct);

            if (conflictingToken is not null)
                return new(false, null, true, conflictingToken);

            var addedToken = _unitOfWork.EmailConfirmationTokenRepository.Add(userId, generatedToken);

            var checkResult = await _unitOfWork.SaveChangesAsync(ct);

            if (checkResult != 1)
                return new(false, null, false, null);

            return new(true, addedToken, false, null);
        }
        catch (Exception e)
        {
            var numberedException = new NumberedException(e);
            _logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
                nameof(EmailConfirmationTokenService), nameof(CheckConflictAndAdd));
            throw;
        }
    }

    public async Task<EmailConfirmationTokenValidationResult> CheckTokenValidity(Guid userId, string token,
        CancellationToken ct = default)
    {
        try
        {
            var targetToken = await _unitOfWork.EmailConfirmationTokenRepository.GetByTokenValue(userId, token, ct);

            if (targetToken is null)
                return new(false, false);

            return new(true, EmailConfirmationToken.ValidateToken(targetToken));
        }
        catch (Exception e)
        {
            var numberedException = new NumberedException(e);
            _logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
                nameof(EmailConfirmationTokenService), nameof(CheckTokenValidity));
            throw;
        }
    }

    public async Task<UpdateRecordResult<EmailConfirmationToken?>> FindUserAndInvalidateAllTokenByUserId(
        Guid userId,
        CancellationToken ct = default)
    {
        try
        {
            var targetUser =
                await _unitOfWork.ListUserRepository.GetWithoutDetailsByIdAsync(userId, ct);

            if (targetUser is null)
                return new(false, false, false, null);

            var foundTokensCount =
                await _unitOfWork.EmailConfirmationTokenRepository.InvalidateAllByUserIdAsync(userId, ct);

            var checkResult = await _unitOfWork.SaveChangesAsync(ct);

            if (checkResult != foundTokensCount)
                return new(true, false, false, null);

            return new(true, true, false, null);
        }
        catch (Exception e)
        {
            var numberedException = new NumberedException(e);
            _logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
                nameof(EmailConfirmationTokenService), nameof(FindUserAndInvalidateAllTokenByUserId));
            throw;
        }
    }

    public async Task<UpdateRecordResult<EmailConfirmationToken?>> FindUserAndInvalidateAllTokenByUserEmail(
        string userEmailAddress,
        CancellationToken ct = default)
    {
        try
        {
            var targetUser =
                await _unitOfWork.ListUserRepository.GetWithoutDetailsByEmailAddressAsync(userEmailAddress, ct);

            if (targetUser is null)
                return new(false, false, false, null);

            var foundTokensCount =
                await _unitOfWork.EmailConfirmationTokenRepository.InvalidateAllByUserIdAsync(targetUser.UserId, ct);

            var checkResult = await _unitOfWork.SaveChangesAsync(ct);

            if (checkResult != foundTokensCount)
                return new(true, false, false, null);

            return new(true, true, false, null);
        }
        catch (Exception e)
        {
            var numberedException = new NumberedException(e);
            _logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
                nameof(EmailConfirmationTokenService), nameof(FindUserAndInvalidateAllTokenByUserEmail));
            throw;
        }
    }

    public async Task<UpdateRecordResult<EmailConfirmationToken?>> FindAndMarkTokenAsUsedById(Guid userId,
        Guid emailConfirmationTokenId, CancellationToken ct = default)
    {
        try
        {
            var targetToken =
                await _unitOfWork.EmailConfirmationTokenRepository.GetByIdWithoutUserDetails(userId,
                    emailConfirmationTokenId, ct);

            if (targetToken is null)
                return new(false, false, false, null);

            _unitOfWork.EmailConfirmationTokenRepository.MarkTokenAsUsed(targetToken);

            var checkResult = await _unitOfWork.SaveChangesAsync(ct);

            if (checkResult != 1)
                return new(true, false, false, null);

            return new(true, true, false, null);
        }
        catch (Exception e)
        {
            var numberedException = new NumberedException(e);
            _logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
                nameof(EmailConfirmationTokenService), nameof(FindAndMarkTokenAsUsedById));
            throw;
        }
    }

    public async Task<UpdateRecordResult<EmailConfirmationToken?>> FindAndMarkTokenAsUsedByTokenValue(Guid userId,
        string tokenValue, CancellationToken ct = default)
    {
        try
        {
            var targetToken =
                await _unitOfWork.EmailConfirmationTokenRepository.GetByTokenValue(userId, tokenValue, ct);

            if (targetToken is null)
                return new(false, false, false, null);

            _unitOfWork.EmailConfirmationTokenRepository.MarkTokenAsUsed(targetToken);

            var checkResult = await _unitOfWork.SaveChangesAsync(ct);

            if (checkResult != 1)
                return new(true, false, false, null);

            return new(true, true, false, null);
        }
        catch (Exception e)
        {
            var numberedException = new NumberedException(e);
            _logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
                nameof(EmailConfirmationTokenService), nameof(FindAndMarkTokenAsUsedByTokenValue));
            throw;
        }
    }

    public async Task<RemoveRecordResult> FindAndDeleteByTokenValueAsync(Guid userId, string token,
        CancellationToken ct = default)
    {
        try
        {
            var targetToken = await _unitOfWork.EmailConfirmationTokenRepository.GetByTokenValue(userId, token, ct);
            if (targetToken is null)
                return new(false, false, 0);

            _unitOfWork.EmailConfirmationTokenRepository.Delete(targetToken);

            var checkResult = await _unitOfWork.SaveChangesAsync(ct);

            if (checkResult != 1)
                return new(true, false, 0);

            return new(true, true, 1);
        }
        catch (Exception e)
        {
            var numberedException = new NumberedException(e);
            _logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
                nameof(EmailConfirmationTokenService), nameof(FindAndDeleteByTokenValueAsync));
            throw;
        }
    }

    public async Task<RemoveRecordResult> FindAndDeleteByIdAsync(Guid userId, Guid emailConfirmationTokenId,
        CancellationToken ct = default)
    {
        try
        {
            var targetToken =
                await _unitOfWork.EmailConfirmationTokenRepository.GetByIdWithoutUserDetails(userId,
                    emailConfirmationTokenId, ct);
            if (targetToken is null)
                return new(false, false, 0);

            _unitOfWork.EmailConfirmationTokenRepository.Delete(targetToken);

            var checkResult = await _unitOfWork.SaveChangesAsync(ct);

            if (checkResult != 1)
                return new(true, false, 0);

            return new(true, true, 1);
        }
        catch (Exception e)
        {
            var numberedException = new NumberedException(e);
            _logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
                nameof(EmailConfirmationTokenService), nameof(FindAndDeleteByIdAsync));
            throw;
        }
    }

    public async Task<RemoveRecordResult> DeleteAllUsedByUserIdAsync(
        Guid userId, CancellationToken ct = default)
    {
        try
        {
            var targetUser = await _unitOfWork.ListUserRepository.GetWithoutDetailsByIdAsync(userId, ct);

            if (targetUser is null)
                return new(false, false, 0);

            var emailConfirmationTokensToBeRemoved = await
                _unitOfWork.EmailConfirmationTokenRepository.GetAllByUserIdAsync(userId, ValidityCheck.IsNotValid,
                    ct);

            if (emailConfirmationTokensToBeRemoved.Count == 0)
                return new(true, true, 0);

            _unitOfWork.EmailConfirmationTokenRepository.DeleteBatch(emailConfirmationTokensToBeRemoved);

            var checkResult = await _unitOfWork.SaveChangesAsync(ct);

            if (checkResult != emailConfirmationTokensToBeRemoved.Count)
                return new(true, false, checkResult);

            return new(true, true, checkResult);
        }
        catch (Exception e)
        {
            var numberedException = new NumberedException(e);
            _logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
                nameof(EmailConfirmationTokenService), nameof(DeleteAllUsedByUserIdAsync));
            throw;
        }
    }

    public async Task<RemoveRecordResult> DeleteAllUsedByUserEmailAsync(
        string userEmailAddress, CancellationToken ct = default)
    {
        try
        {
            var targetUser =
                await _unitOfWork.ListUserRepository.GetWithoutDetailsByEmailAddressAsync(userEmailAddress, ct);

            if (targetUser is null)
                return new(false, false, 0);

            var emailConfirmationTokensToBeRemoved = await
                _unitOfWork.EmailConfirmationTokenRepository.GetAllByUserIdAsync(targetUser.UserId,
                    ValidityCheck.IsNotValid,
                    ct);

            if (emailConfirmationTokensToBeRemoved.Count == 0)
                return new(true, true, 0);

            _unitOfWork.EmailConfirmationTokenRepository.DeleteBatch(emailConfirmationTokensToBeRemoved);

            var checkResult = await _unitOfWork.SaveChangesAsync(ct);

            if (checkResult != emailConfirmationTokensToBeRemoved.Count)
                return new(true, false, checkResult);

            return new(true, true, checkResult);
        }
        catch (Exception e)
        {
            var numberedException = new NumberedException(e);
            _logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
                nameof(EmailConfirmationTokenService), nameof(DeleteAllUsedByUserEmailAsync));
            throw;
        }
    }
}