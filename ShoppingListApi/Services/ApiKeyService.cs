using ShoppingListApi.Configs;
using ShoppingListApi.Exceptions;
using ShoppingListApi.Interfaces.Services;
using ShoppingListApi.Model.Entity;
using ShoppingListApi.Model.ReturnTypes;

namespace ShoppingListApi.Services;

public class ApiKeyService(IUnitOfWork unitOfWork, ILogger<ApiKeyService> logger) : IApiKeyService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ILogger<ApiKeyService> _logger = logger;

    public async Task<AddRecordResult<ApiKey?, ApiKey?>> CreateAsync(Guid userId, CancellationToken ct = default)
    {
        try
        {
            var newKey = ApiKey.GenerateKey();
            var conflictingKey = await _unitOfWork.ApiKeyRepository.GetByKeyAsync(userId, newKey, ct);

            if (conflictingKey is not null)
                return new(false, null, true, conflictingKey);

            var apiKey = await _unitOfWork.ApiKeyRepository.CreateAsync(userId, newKey, ct);

            var checkResult = await _unitOfWork.SaveChangesAsync(ct);

            if (checkResult != 1)
            {
                _logger.LogError("Failed to create API key for user {UserId}. SaveChangesAsync returned {Result}.",
                    userId, checkResult);
                return new(false, null, false, null);
            }

            return new(true, apiKey, false, null);
        }
        catch (Exception e)
        {
            var numberedException = new NumberedException(e);
            _logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
                nameof(ApiKeyService), nameof(CreateAsync));
            throw numberedException;
        }
    }

    public async Task<UpdateRecordResult<ApiKey?>> FindAndInvalidateAsync(Guid userId, Guid apiKeyId,
        CancellationToken ct = default)
    {
        try
        {
            var targetApiKey = await _unitOfWork.ApiKeyRepository.GetWithoutDetailsByIdAsync(userId, apiKeyId, ct);

            if (targetApiKey is null)
                return new(false, false, true, null);

            if (targetApiKey.IsValid is false)
                return new(true, true, false, null);

            _unitOfWork.ApiKeyRepository.Invalidate(targetApiKey);

            var checkResult = await _unitOfWork.SaveChangesAsync(ct);

            if (checkResult != 1)
                return new(true, false, false, null);

            return new(true, true, false, null);
        }
        catch (Exception e)
        {
            var numberedException = new NumberedException(e);
            _logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
                nameof(ApiKeyService), nameof(FindAndInvalidateAsync));
            throw numberedException;
        }
    }

    public async Task<UpdateRecordResult<object?>> FindUserAndInvalidateAllByUserIdAsync(
        Guid userId, CancellationToken ct = default)
    {
        try
        {
            var targetUser = await _unitOfWork.ListUserRepository.GetWithoutDetailsByIdAsync(userId, ct);

            if (targetUser is null) return new(false, false, false, null);

            var apiKeysFountCount = await _unitOfWork.ApiKeyRepository.InvalidateAllByUserIdAsync(userId, ct);

            var checkResult = await _unitOfWork.SaveChangesAsync(ct);

            if (checkResult != apiKeysFountCount)
                return new(true, false, false, null);

            return new(true, true, false, null);
        }
        catch (Exception e)
        {
            var numberedException = new NumberedException(e);
            _logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
                nameof(ApiKeyService), nameof(FindUserAndInvalidateAllByUserIdAsync));
            throw numberedException;
        }
    }

    public async Task<RemoveRecordResult> FindAndDeleteAsync(Guid userId, Guid apiKeyId, CancellationToken ct = default)
    {
        try
        {
            var targetApiKey = await _unitOfWork.ApiKeyRepository.GetWithoutDetailsByIdAsync(userId, apiKeyId, ct);

            if (targetApiKey is null)
                return new(false, false, 0);

            _unitOfWork.ApiKeyRepository.Delete(targetApiKey);

            var checkResult = await _unitOfWork.SaveChangesAsync(ct);

            if (checkResult != 1)
                return new(true, false, 0);

            return new(true, true, 1);
        }
        catch (Exception e)
        {
            var numberedException = new NumberedException(e);
            _logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
                nameof(ApiKeyService), nameof(FindAndDeleteAsync));
            throw numberedException;
        }
    }

    public async Task<RemoveRecordResult> DeleteExpiredAsync(CancellationToken ct = default)
    {
        try
        {
            var expiredApiKeys = await _unitOfWork.ApiKeyRepository.GetAllInvalidatedKeysBeforeDateAsync(ct);

            if (expiredApiKeys.Count == 0)
                return new(false, true, 0);

            _unitOfWork.ApiKeyRepository.DeleteBatch(expiredApiKeys);

            var checkResult = await _unitOfWork.SaveChangesAsync(ct);

            if (checkResult != expiredApiKeys.Count)
                return new(true, false, 0);

            return new(true, true, checkResult);
        }
        catch (Exception e)
        {
            var numberedException = new NumberedException(e);
            _logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
                nameof(ApiKeyService), nameof(DeleteExpiredAsync));
            throw numberedException;
        }
    }
}