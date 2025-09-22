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

        [HttpGet]
        [AdminEndpoint]
        [Route("apiKeyId:guid/User/userId:guid/")]
        public async Task<ActionResult> GetApiKeyById([FromRoute] Guid apiKeyId, [FromRoute] Guid userId)
        {
            var ct = CancellationTokenSource
                .CreateLinkedTokenSource(_hostApplicationLifetime.ApplicationStopping, HttpContext.RequestAborted)
                .Token;

            var apiKeyEntity = await _apiKeyService.GetWithoutDetailsByIdAsync(userId, apiKeyId, ct);

            if (apiKeyEntity is null)
                return NotFound(new ResponseResult<Dictionary<string, Guid>>(
                    new Dictionary<string, Guid>
                        { { nameof(apiKeyId), apiKeyId }, { nameof(userId), apiKeyEntity!.UserId } },
                    "API key for the provided ids was not found."));

            return Ok(apiKeyEntity);
        }

        [HttpPatch]
        [AdminEndpoint]
        [Route("apiKeyId:guid/User/userId:guid/invalidate")]
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

        [HttpPatch]
        [AdminEndpoint]
        [Route("User/userId:guid/invalidateAll")]
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

        [HttpDelete]
        [AdminEndpoint]
        [Route("expired")]
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