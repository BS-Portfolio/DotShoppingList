using Microsoft.AspNetCore.Mvc;
using ShoppingListApi.Attributes;
using ShoppingListApi.Configs;
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

        /// <summary>
        /// [PublicEndpoint] - Authenticates a user and returns user details if successful.
        /// Use this endpoint to log in with email and password in base64 format.
        /// </summary>
        /// <param name="loginDataDto">The login credentials (email and password).</param>
        [HttpPost]
        [Route("login")]
        [PublicEndpoint]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseResult<ListUserGetDto>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(AuthenticationErrorResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResponseResult<string>))]
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
                    _logger.LogError(
                        "Failed to generate API key for user {EmailAddress} during login. This might indicate a problem with the database.",
                        loginDataDto.EmailAddress);

                return StatusCode(StatusCodes.Status500InternalServerError, new ResponseResult<string>(
                    loginDataDto.EmailAddress,
                    "An internal server error occurred logging you in. Please try again later."));
            }

            return Ok(new ResponseResult<ListUserGetDto>(loginResult.TargetUser!, "Login successful."));
        }

        /// <summary>
        /// [UserEndpoint] - Logs out a user by invalidating their API key.
        /// Use this endpoint to log out by providing your user ID and API key.
        /// </summary>
        [HttpPost]
        [Route("logout")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseResult<Guid>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResponseResult<object?>))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(AuthenticationErrorResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ResponseResult<object?>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResponseResult<Guid>))]
        public async Task<ActionResult> Logout()
        {
            var checkAccessResult = this.CheckAccess();

            if (checkAccessResult.ActionResult is not null)
                return checkAccessResult.ActionResult;

            var ct = CancellationTokenSource
                .CreateLinkedTokenSource(_hostApplicationLifetime.ApplicationStopping, HttpContext.RequestAborted)
                .Token;

            var logoutResult = await _appAuthenticationService.LogOut(checkAccessResult.RequestingUserId!.Value,
                checkAccessResult.ApiKey!, ct);

            if (logoutResult.Success is not true)
            {
                if (logoutResult.ApiKeyFound is not true)
                    return NotFound(new ResponseResult<object?>(null, "API key not found!"));

                if (logoutResult.UserOwnsApiKey is false)
                {
                    if (logoutResult.InvalidationSuccessful is not true)
                        _logger.LogError(
                            "Failed to invalidate API key {ApiKey} to avoid misuse. This might indicate a problem with the database.",
                            checkAccessResult.ApiKey);

                    return StatusCode(403,
                        new AuthenticationErrorResponse(AuthorizationErrorEnum.IdAndKeyNotMatching));
                }

                return StatusCode(500, new ResponseResult<Guid>(checkAccessResult.RequestingUserId.Value,
                    "An internal server error occurred logging you out. Please try again later."));
            }

            return Ok(new ResponseResult<Guid>(checkAccessResult.RequestingUserId.Value, "Logout successful."));
        }
    }
}