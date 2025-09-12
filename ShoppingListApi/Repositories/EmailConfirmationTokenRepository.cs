using Microsoft.EntityFrameworkCore;
using ShoppingListApi.Data.Contexts;
using ShoppingListApi.Enums;
using ShoppingListApi.Interfaces.Repositories;
using ShoppingListApi.Model.Entity;
using ShoppingListApi.Model.ReturnTypes;

namespace ShoppingListApi.Repositories;

public class EmailConfirmationTokenRepository(
    AppDbContext appDbContext,
    ILogger<EmailConfirmationTokenRepository> logger) : IEmailConfirmationTokenRepository
{
    private readonly AppDbContext _appDbContext = appDbContext;
    private readonly ILogger<EmailConfirmationTokenRepository> _logger = logger;

    public async Task<List<EmailConfirmationToken>> GetAllByUserIdAsync(Guid userId, ValidityCheck validityCheck,
        CancellationToken ct = default)
    {
        bool? validityCheckValue = validityCheck switch
        {
            ValidityCheck.IsValid => true,
            ValidityCheck.IsNotValid => false,
            _ => null
        };
        var query = _appDbContext.EmailConfirmationTokens.Where(t => t.UserId == userId);

        if (validityCheckValue.HasValue && validityCheckValue.Value)
            query = query.Where(t => t.IsUsed == false && t.ExpirationDateTime > DateTimeOffset.UtcNow);

        if (validityCheckValue is false)
            query = query.Where(t => t.IsUsed == true || t.ExpirationDateTime <= DateTimeOffset.UtcNow);

        return await query.ToListAsync(ct);
    }

    public async Task<EmailConfirmationToken?> GetByIdWithUserDetails(Guid emailConfirmationTokenId,
        CancellationToken ct = default)
    {
        return await _appDbContext.EmailConfirmationTokens.FindAsync([emailConfirmationTokenId], ct);
    }

    public async Task<EmailConfirmationToken?> GetByTokenValue(Guid userId, string token,
        CancellationToken ct = default)
    {
        return await _appDbContext.EmailConfirmationTokens
            .FirstOrDefaultAsync(t => t.UserId == userId && t.Token == token, ct);
    }

    public async Task<EmailConfirmationToken?> AddAsync(Guid userId, string generatedToken,
        CancellationToken ct = default)
    {
        var newTokenId = Guid.NewGuid();

        var newToken = new EmailConfirmationToken
        {
            EmailConfirmationTokenId = newTokenId,
            UserId = userId,
            Token = generatedToken,
            CreationDateTime = DateTimeOffset.UtcNow,
            ExpirationDateTime = DateTimeOffset.UtcNow.AddHours(24), // Token valid for 24 hours
            IsUsed = false
        };

        _appDbContext.EmailConfirmationTokens.Add(newToken);

        var checkResult = await _appDbContext.SaveChangesAsync(ct);

        if (checkResult <= 0)
            return null;

        return newToken;
    }

    public async Task<UpdateRecordResult<object?>> MarkTokenAsUsedAsync(Guid userId, Guid emailConfirmationTokenId,
        CancellationToken ct = default)
    {
        var existingToken = await _appDbContext.EmailConfirmationTokens
            .FirstOrDefaultAsync(t => t.UserId == userId && t.EmailConfirmationTokenId == emailConfirmationTokenId, ct);

        if (existingToken is null)
            return new(false, false, false, null);

        existingToken.IsUsed = true;

        if (existingToken.ExpirationDateTime > DateTimeOffset.UtcNow)
            existingToken.ExpirationDateTime = DateTimeOffset.UtcNow;

        var checkResult = await _appDbContext.SaveChangesAsync(ct);

        if (checkResult <= 0)
            return new(true, false, false, null);

        return new(true, true, false, null);
    }

    public async Task<bool> InvalidateAllByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        var tokens = await _appDbContext.EmailConfirmationTokens
            .Where(t => t.UserId == userId && t.IsUsed == false || t.ExpirationDateTime > DateTimeOffset.UtcNow)
            .ToListAsync(ct);

        if (!tokens.Any())
            return true;

        foreach (var token in tokens)
        {
            token.IsUsed = true;
            token.ExpirationDateTime = DateTimeOffset.UtcNow;
        }

        var checkResult = await _appDbContext.SaveChangesAsync(ct);

        if (checkResult <= 0)
            return false;

        return true;
    }

    public async Task<RemoveRecordResult> DeleteAsync(Guid userId, Guid emailConfirmationTokenId,
        CancellationToken ct = default)
    {
        var existingToken = await _appDbContext.EmailConfirmationTokens
            .FirstOrDefaultAsync(t => t.UserId == userId && t.EmailConfirmationTokenId == emailConfirmationTokenId, ct);

        if (existingToken is null)
            return new RemoveRecordResult(false, false, 0);

        _appDbContext.EmailConfirmationTokens.Remove(existingToken);

        var checkResult = await _appDbContext.SaveChangesAsync(ct);

        if (checkResult <= 0)
            return new RemoveRecordResult(true, false, 0);

        return new RemoveRecordResult(true, true, checkResult);
    }

    public async Task<RemoveRecordResult> DeleteBatchAsync(List<Guid> emailConfirmationTokenIds,
        CancellationToken ct = default)
    {
        var tokensToDelete = await _appDbContext.EmailConfirmationTokens
            .Where(t => emailConfirmationTokenIds.Contains(t.EmailConfirmationTokenId))
            .ToListAsync(ct);

        if (!tokensToDelete.Any())
            return new RemoveRecordResult(false, true, 0);

        _appDbContext.EmailConfirmationTokens.RemoveRange(tokensToDelete);

        var checkResult = await _appDbContext.SaveChangesAsync(ct);

        if (checkResult != tokensToDelete.Count)
            return new RemoveRecordResult(true, false, checkResult);

        return new RemoveRecordResult(true, true, checkResult);
    }
}