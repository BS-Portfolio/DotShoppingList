using Microsoft.AspNetCore.Mvc;
using ShoppingListApi.Enums;
using ShoppingListApi.Interfaces.Services;
using ShoppingListApi.Model.DTOs.Get;
using ShoppingListApi.Model.DTOs.Post;
using ShoppingListApi.Model.ReturnTypes;

namespace ShoppingListApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AuthController(
        IAppAuthenticationService appAuthenticationService,
        IHostApplicationLifetime hostApplicationLifetime,
        ILogger<AuthController> logger) : ControllerBase
    {
        private readonly IAppAuthenticationService _appAuthenticationService =
            appAuthenticationService;

        private readonly IHostApplicationLifetime _hostApplicationLifetime = hostApplicationLifetime;

        private readonly ILogger<AuthController> _logger = logger;

        [HttpPost]
        [Route("login")]
        public async Task<ActionResult> Login([FromBody] LoginDataDto loginDataDto)
        {
            var ct = CancellationTokenSource
                .CreateLinkedTokenSource(_hostApplicationLifetime.ApplicationStopping, HttpContext.RequestAborted)
                .Token;
            var loginResult = await _appAuthenticationService.LogIn(loginDataDto, ct);

            if (loginResult.LoginSuccessful is not true)
            {
                if (loginResult is { AccountExists: not true } or { PasswordIsValid: not true })
                    return Unauthorized(new AuthenticationErrorResponse(AuthorizationErrorEnum.LoginFailure));

                if (loginResult.ApiKeyGenerationSuccessful is not true)
                {
                    _logger.LogError(
                        "Failed to generate API key for user {EmailAddress} during login. This might indicate a problem with the database.",
                        loginDataDto.EmailAddress);
                }

                return StatusCode(StatusCodes.Status500InternalServerError, new ResponseResult<string>(
                    loginDataDto.EmailAddress,
                    "An internal server error occurred logging you in. Please try again later."));
            }

            return Ok(new ResponseResult<ListUserGetDto>(loginResult.TargetUser!, "Login successful."));
        }

        [HttpPost]
        [Route("logout")]
        public async Task<ActionResult> Logout([FromHeader(Name = "USER-ID")] Guid? requestingUserId, [FromHeader(Name =
                "USER-KEY")]
            string? apiKey)
        {
            if (requestingUserId is null || requestingUserId == Guid.Empty || string.IsNullOrWhiteSpace(apiKey))
                return BadRequest(new ResponseResult<object?>(null, "Logout credentials missing!"));

            var ct = CancellationTokenSource
                .CreateLinkedTokenSource(_hostApplicationLifetime.ApplicationStopping, HttpContext.RequestAborted)
                .Token;

            var userId = requestingUserId.Value;

            // expire API key
            var logoutResult = await _appAuthenticationService.LogOut(userId, apiKey, ct);

            if (logoutResult.Success is not true)
            {
                if (logoutResult.ApiKeyFound is not true)
                    return NotFound(new ResponseResult<object?>(null, "API key not found!"));

                if (logoutResult.UserOwnsApikey is false)
                {
                    if (logoutResult.InvalidationSuccessful is not true)
                        _logger.LogError(
                            "Failed to invalidate API key {ApiKey} to avoid misuse. This might indicate a problem with the database.",
                            apiKey);

                    return StatusCode(403, new AuthenticationErrorResponse(AuthorizationErrorEnum.IdAndKeyNotMatching));
                }

                return StatusCode(500, new ResponseResult<Guid>(userId,
                    "An internal server error occurred logging you out. Please try again later."));
            }

            return Ok(new ResponseResult<Guid>(userId, "Logout successful."));
        }
    }
}