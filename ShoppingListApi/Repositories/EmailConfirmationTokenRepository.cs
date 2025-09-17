using Microsoft.EntityFrameworkCore;
using ShoppingListApi.Data.Contexts;
using ShoppingListApi.Enums;
using ShoppingListApi.Interfaces.Repositories;
using ShoppingListApi.Model.Entity;

namespace ShoppingListApi.Repositories;

public class EmailConfirmationTokenRepository(AppDbContext appDbContext) : IEmailConfirmationTokenRepository
{
    private readonly AppDbContext _appDbContext = appDbContext;

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

    public async Task<EmailConfirmationToken?> GetByIdWithUserDetails(Guid userId, Guid emailConfirmationTokenId,
        CancellationToken ct = default)
    {
        return await _appDbContext.EmailConfirmationTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(
                t => t.UserId == userId && t.EmailConfirmationTokenId == emailConfirmationTokenId, ct);
    }

    public async Task<EmailConfirmationToken?> GetByIdWithoutUserDetails(Guid userId, Guid emailConfirmationTokenId,
        CancellationToken ct = default)
    {
        return await _appDbContext.EmailConfirmationTokens.FirstOrDefaultAsync(
            t => t.UserId == userId && t.EmailConfirmationTokenId == emailConfirmationTokenId, ct);
    }

    public async Task<EmailConfirmationToken?> GetByTokenValue(Guid userId, string token,
        CancellationToken ct = default)
    {
        return await _appDbContext.EmailConfirmationTokens
            .FirstOrDefaultAsync(t => t.UserId == userId && t.Token == token, ct);
    }

    public EmailConfirmationToken Add(Guid userId, string generatedToken)
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

        return newToken;
    }

    public void MarkTokenAsUsed(EmailConfirmationToken targetEmailConfirmationToken)
    {
        targetEmailConfirmationToken.IsUsed = true;

        if (targetEmailConfirmationToken.ExpirationDateTime > DateTimeOffset.UtcNow)
            targetEmailConfirmationToken.ExpirationDateTime = DateTimeOffset.UtcNow;
    }

    public async Task<int> InvalidateAllByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        var tokens = await _appDbContext.EmailConfirmationTokens
            .Where(t => t.UserId == userId && (t.IsUsed == false || t.ExpirationDateTime > DateTimeOffset.UtcNow))
            .ToListAsync(ct);

        if (tokens.Count == 0)
            return 0;

        foreach (var token in tokens)
        {
            token.IsUsed = true;
            token.ExpirationDateTime = DateTimeOffset.UtcNow;
        }

        return tokens.Count;
    }

    public void Delete(EmailConfirmationToken targetEmailConfirmationToken)
    {
        _appDbContext.EmailConfirmationTokens.Remove(targetEmailConfirmationToken);
    }

    public void DeleteBatch(List<EmailConfirmationToken> emailConfirmationTokens)
    {
        _appDbContext.EmailConfirmationTokens.RemoveRange(emailConfirmationTokens);
    }
}