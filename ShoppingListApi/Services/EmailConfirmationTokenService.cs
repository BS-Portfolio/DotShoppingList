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
}