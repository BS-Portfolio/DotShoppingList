using Microsoft.AspNetCore.Mvc;
using ShoppingListApi.Attributes;
using ShoppingListApi.Interfaces.Services;
using ShoppingListApi.Model.ReturnTypes;

namespace ShoppingListApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ApiKeyController(
        ILogger<ApiKeyController> logger,
        IHostApplicationLifetime hostApplicationLifetime,
        IApiKeyService apiKeyService) : ControllerBase
    {
        private readonly ILogger<ApiKeyController> _logger = logger;

        private readonly IApiKeyService _apiKeyService = apiKeyService;

        private readonly IHostApplicationLifetime _hostApplicationLifetime = hostApplicationLifetime;

        /// <summary>
        /// [AdminEndpoint] - Retrieves an API key by its ID for a specific user. Admin endpoint.
        /// - Returns 200 OK with the API key if found.
        /// - Returns 404 Not Found if the API key does not exist for the user.
        /// - Returns 500 Internal Server Error for unexpected issues.
        /// Use this endpoint to get an API key by its ID and user ID as an admin.
        /// </summary>
        /// <param name="apiKeyId">The ID of the API key to retrieve.</param>
        /// <param name="userId">The ID of the user who owns the API key.</param>
        [HttpGet]
        [AdminEndpoint]
        [Route("{apiKeyId:guid}/User/{userId:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseResult<object>))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ResponseResult<Dictionary<string, Guid>>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResponseResult<object?>))]
        public async Task<ActionResult> GetApiKeyById([FromRoute] Guid apiKeyId, [FromRoute] Guid userId)
        {
            var ct = CancellationTokenSource
                .CreateLinkedTokenSource(_hostApplicationLifetime.ApplicationStopping, HttpContext.RequestAborted)
                .Token;

            var apiKeyEntity = await _apiKeyService.GetWithoutDetailsByIdAsync(userId, apiKeyId, ct);

            if (apiKeyEntity is null)
                return NotFound(new ResponseResult<Dictionary<string, Guid>>(
                    new Dictionary<string, Guid>
                        { { nameof(apiKeyId), apiKeyId }, { nameof(userId), userId } },
                    "API key for the provided ids was not found."));

            return Ok(apiKeyEntity);
        }

        /// <summary>
        /// [AdminEndpoint] - Invalidates a specific API key for a user. Admin endpoint.
        /// - Returns 200 OK if the API key was successfully invalidated.
        /// - Returns 404 Not Found if the API key does not exist for the user.
        /// - Returns 500 Internal Server Error for unexpected issues.
        /// Use this endpoint to invalidate an API key by its ID and user ID as an admin.
        /// </summary>
        /// <param name="apiKeyId">The ID of the API key to invalidate.</param>
        /// <param name="userId">The ID of the user who owns the API key.</param>
        [HttpPatch]
        [AdminEndpoint]
        [Route("{apiKeyId:guid}/User/{userId:guid}/invalidate")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseResult<Dictionary<string, Guid>>))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ResponseResult<Dictionary<string, Guid>>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError,
            Type = typeof(ResponseResult<Dictionary<string, Guid>>))]
        public async Task<ActionResult> InvalidateApiKey([FromRoute] Guid apiKeyId, [FromRoute] Guid userId)
        {
            var ct = CancellationTokenSource
                .CreateLinkedTokenSource(_hostApplicationLifetime.ApplicationStopping, HttpContext.RequestAborted)
                .Token;

            var result = await _apiKeyService.FindAndInvalidateAsync(userId, apiKeyId, ct);

            if (result.Success is not true)
            {
                if (result.TargetExists is not true)
                {
                    return NotFound(new ResponseResult<Dictionary<string, Guid>>(
                        new Dictionary<string, Guid>
                        {
                            { nameof(apiKeyId), apiKeyId },
                            { nameof(userId), userId }
                        },
                        "API key for the provided ids was not found."));
                }

                return StatusCode(500, new ResponseResult<Dictionary<string, Guid>>(
                    new Dictionary<string, Guid>
                    {
                        { nameof(apiKeyId), apiKeyId },
                        { nameof(userId), userId }
                    },
                    "An error occurred while trying to invalidate the API key."));
            }

            return Ok(new ResponseResult<Dictionary<string, Guid>>(
                new Dictionary<string, Guid>
                {
                    { nameof(apiKeyId), apiKeyId },
                    { nameof(userId), userId }
                },
                "API key was successfully invalidated."));
        }

        /// <summary>
        /// [AdminEndpoint] - Invalidates all API keys for a specific user. Admin endpoint.
        /// - Returns 200 OK if all API keys were successfully invalidated.
        /// - Returns 204 No Content if the user does not exist or has no API keys.
        /// - Returns 500 Internal Server Error for unexpected issues.
        /// Use this endpoint to invalidate all API keys for a user as an admin.
        /// </summary>
        /// <param name="userId">The ID of the user whose API keys will be invalidated.</param>
        [HttpPatch]
        [AdminEndpoint]
        [Route("User/{userId:guid}/invalidateAll")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseResult<Guid>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError,
            Type = typeof(ResponseResult<Dictionary<string, Guid>>))]
        public async Task<ActionResult> InvalidateAllApiKeysForUser([FromRoute] Guid userId)
        {
            var ct = CancellationTokenSource
                .CreateLinkedTokenSource(_hostApplicationLifetime.ApplicationStopping, HttpContext.RequestAborted)
                .Token;

            var result = await _apiKeyService.FindUserAndInvalidateAllByUserIdAsync(userId, ct);

            if (result.Success is not true)
            {
                if (result.TargetExists is not true)
                {
                    return NoContent();
                }

                return StatusCode(500, new ResponseResult<Dictionary<string, Guid>>(
                    new Dictionary<string, Guid>
                    {
                        { nameof(userId), userId }
                    },
                    "An error occurred while trying to invalidate all API keys for the user."));
            }

            return Ok(new ResponseResult<Guid>(userId, "All API keys for the user were successfully invalidated."));
        }

        /// <summary>
        /// [AdminEndpoint] - Deletes all expired API keys. Admin endpoint.
        /// - Returns 200 OK with the number of deleted API keys if successful.
        /// - Returns 500 Internal Server Error for unexpected issues.
        /// Use this endpoint to delete all expired API keys as an admin.
        /// </summary>
        [HttpDelete]
        [AdminEndpoint]
        [Route("expired")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseResult<int>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResponseResult<object?>))]
        public async Task<ActionResult> DeleteAllExpiredApiKeys()
        {
            var ct = CancellationTokenSource
                .CreateLinkedTokenSource(_hostApplicationLifetime.ApplicationStopping, HttpContext.RequestAborted)
                .Token;

            var result = await _apiKeyService.DeleteExpiredAsync(ct);

            if (result.Success is not true)
            {
                if (result.TargetExists is not true && result.RecordsAffected == 0)
                {
                    return Ok(new ResponseResult<int>(result.RecordsAffected,
                        " No expired API keys were found to delete."));
                }

                return StatusCode(500, new ResponseResult<object?>(null,
                    "An error occurred while trying to delete all expired API keys."));
            }

            return Ok(new ResponseResult<int>(result.RecordsAffected,
                "All expired API keys were successfully deleted."));
        }
    }
}