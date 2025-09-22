using Microsoft.AspNetCore.Mvc;
using ShoppingListApi.Attributes;
using ShoppingListApi.Enums;
using ShoppingListApi.Interfaces.Services;
using ShoppingListApi.Model.DTOs.Create;
using ShoppingListApi.Model.DTOs.Get;
using ShoppingListApi.Model.DTOs.Patch;
using ShoppingListApi.Model.DTOs.Post;
using ShoppingListApi.Model.Entity;
using ShoppingListApi.Model.ReturnTypes;


namespace ShoppingListApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController(
        IListUserService listUserService,
        IHostApplicationLifetime hostApplicationLifetime,
        ILogger<UserController> logger) : ControllerBase
    {
        private readonly IListUserService _listUserService = listUserService;
        private readonly IHostApplicationLifetime _hostApplicationLifetime = hostApplicationLifetime;
        private readonly ILogger<UserController> _logger = logger;

        /// <summary>
        /// [PublicEndpoint] - Registers a new user account.
        /// - Returns 201 Created with the new user ID if registration is successful.  
        /// - Returns 409 Conflict if the email address is already registered.  
        /// - Returns 500 Internal Server Error for unexpected issues.  
        /// Use this endpoint to create a new user by providing the required registration details.
        /// </summary>
        [HttpPost]
        [PublicEndpoint]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ResponseResult<Guid?>))]
        [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ResponseResult<string?>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResponseResult<object?>))]
        public async Task<ActionResult> Register([FromBody] ListUserPostDto listUserPostDto)
        {
            var listUserCreateDto = new ListUserCreateDto(listUserPostDto);

            var result = await _listUserService.CheckConflictAndCreateUserAsync(listUserCreateDto);

            if (result.Success is not true)
            {
                if (result.Conflicts)
                    return Conflict(new ResponseResult<string?>(result.ConflictingRecord?.EmailAddress,
                        "A user with the same email address already exists in the database. Use a different address to register yourself!"));

                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ResponseResult<object?>(null,
                        "Due to an internal error, your request could not be processed."));
            }

            return StatusCode(StatusCodes.Status201Created,
                new ResponseResult<Guid?>(result.AddedRecord, "User created successfully."));
        }

        /// <summary>
        /// [UserEndpoint] - Modifies the first and/or last name of a user.
        /// - Returns 200 OK if the update is successful.
        /// - Returns 400 Bad Request if no fields are provided to update.
        /// - Returns 403 Forbidden if the user is not authorized to modify the details.
        /// - Returns 404 Not Found if the user does not exist.
        /// - Returns 500 Internal Server Error for unexpected issues.
        /// Use this endpoint to update your first and/or last name by providing the required details.
        /// </summary>
        [HttpPatch]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseResult<ListUserPatchDto>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResponseResult<string?>))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(AuthenticationErrorResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ResponseResult<Guid>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResponseResult<object?>))]
        [Route("User/{userId:Guid}")]
        public async Task<ActionResult> ModifyUserDetails([FromRoute] Guid userId,
            [FromBody] ListUserPatchDto listUserPatchDto, [FromHeader(Name = "USER-ID")] Guid? requestingUserId)
        {
            if (requestingUserId is null)
                return Unauthorized(new AuthenticationErrorResponse(AuthorizationErrorEnum.UserCredentialsMissing));

            if (!userId.Equals(requestingUserId))
                return StatusCode(StatusCodes.Status403Forbidden,
                    new AuthenticationErrorResponse(AuthorizationErrorEnum.ActionNotAllowed));

            if (listUserPatchDto.FirstName is null && listUserPatchDto.LastName is null)
                return BadRequest("Nothing to update!");

            var ct = CancellationTokenSource
                .CreateLinkedTokenSource(HttpContext.RequestAborted, _hostApplicationLifetime.ApplicationStopping)
                .Token;

            var result =
                await _listUserService.CheckAccessAndUpdateNameAsync(requestingUserId.Value, userId, listUserPatchDto,
                    ct);

            if (result.Success is not true)
            {
                if (result.AccessGranted is not true)
                    return StatusCode(StatusCodes.Status403Forbidden,
                        new AuthenticationErrorResponse(AuthorizationErrorEnum.ActionNotAllowed));

                if (result.TargetExists is not true)
                    return NotFound(new ResponseResult<Guid>(userId,
                        "User with the provided user id was not found."));

                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ResponseResult<object?>(null,
                        "Due to an internal error, your request could not be processed."));
            }

            return Ok(new ResponseResult<ListUserPatchDto>(listUserPatchDto, "User details updated successfully."));
        }

        /// <summary>
        /// [UserEndpoint] - Deletes the user account for the specified user ID.
        /// - Returns 200 OK with the number of records deleted if successful.
        /// - Returns 403 Forbidden if the user is not authorized to delete the account.
        /// - Returns 404 Not Found if the user does not exist.
        /// - Returns 500 Internal Server Error for unexpected issues.
        /// Use this endpoint to delete your own user account.
        /// </summary>
        /// <param name="userId">The ID of the user to delete.</param>
        /// <param name="requestingUserId">The ID of the user making the request (from header).</param>
        [HttpDelete]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseResult<int>))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(AuthenticationErrorResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ResponseResult<Guid>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(AuthenticationErrorResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResponseResult<object?>))]
        [Route("{userId:guid}")]
        public async Task<ActionResult> DeleteAccount([FromRoute] Guid userId,
            [FromHeader(Name = "USER-ID")] Guid? requestingUserId)
        {
            if (requestingUserId is null)
                return Unauthorized(new AuthenticationErrorResponse(AuthorizationErrorEnum.UserCredentialsMissing));

            if (!requestingUserId.Equals(userId))
                return StatusCode(StatusCodes.Status403Forbidden,
                    new AuthenticationErrorResponse(AuthorizationErrorEnum.ActionNotAllowed));

            var ct = CancellationTokenSource
                .CreateLinkedTokenSource(HttpContext.RequestAborted, _hostApplicationLifetime.ApplicationStopping)
                .Token;

            var result = await _listUserService.CheckAccessAndDeleteUserAsync(requestingUserId.Value, userId, ct);

            if (result.Success is not true)
            {
                if (result.AccessGranted is not true)
                    return StatusCode(StatusCodes.Status403Forbidden,
                        new AuthenticationErrorResponse(AuthorizationErrorEnum.ActionNotAllowed));

                if (result.TargetExists is false)
                    return NotFound(new ResponseResult<Guid>(userId,
                        "User with the provided user id was not found."));

                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ResponseResult<object?>(null,
                        "Due to an internal error, your request could not be processed."));
            }

            return Ok(new ResponseResult<int>(result.RecordsAffected,
                "User successfully deleted. The amount of removed records is returned in the response body."));
        }

        /// <summary>
        /// [AdminEndpoint] - Retrieves user details by email address. Admin endpoint.
        /// - Returns 200 OK with user details if found.
        /// - Returns 404 Not Found if the user does not exist.
        /// - Returns 500 Internal Server Error for unexpected issues.
        /// Use this endpoint to get user details by email address as an admin.
        /// </summary>
        /// <param name="emailAddress">The email address of the user to retrieve.</param>
        [HttpGet]
        [AdminEndpoint]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseResult<ListUser>))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ResponseResult<string>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResponseResult<object?>))]
        [Route("EmailAddress/{emailAddress}")]
        public async Task<ActionResult> GetUserByEmail([FromRoute] string emailAddress)
        {
            var ct = CancellationTokenSource
                .CreateLinkedTokenSource(HttpContext.RequestAborted, _hostApplicationLifetime.ApplicationStopping)
                .Token;

            var user = await _listUserService.GetWitDetailsByEmailAddressAsync(emailAddress, ct);

            if (user is null)
                return NotFound(new ResponseResult<string>(emailAddress,
                    "User with the provided email address was not found."));

            user.PasswordHash = "[REDACTED]";
            return Ok(new ResponseResult<ListUser>(user, "User found and retrieved."));
        }

        /// <summary>
        /// [AdminEndpoint] - Retrieves user details by user ID. Admin endpoint.
        /// - Returns 200 OK with user details if found.
        /// - Returns 404 Not Found if the user does not exist.
        /// - Returns 500 Internal Server Error for unexpected issues.
        /// Use this endpoint to get user details by user ID as an admin.
        /// </summary>
        /// <param name="userId">The ID of the user to retrieve.</param>
        [HttpGet]
        [AdminEndpoint]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseResult<ListUser>))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ResponseResult<Guid>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResponseResult<object?>))]
        [Route("{userId:Guid}/leave")]
        public async Task<ActionResult> GetUserById(Guid userId)
        {
            var ct = CancellationTokenSource
                .CreateLinkedTokenSource(HttpContext.RequestAborted, _hostApplicationLifetime.ApplicationStopping)
                .Token;

            var user = await _listUserService.GetWithDetailsByIdAsync(userId, ct);

            if (user is null)
                return NotFound(new ResponseResult<Guid>(userId,
                    "User with the provided user id was not found."));

            user.PasswordHash = "[REDACTED]";
            return Ok(new ResponseResult<ListUser>(user, "User found and retrieved."));
        }

        /// <summary>
        /// [AdminEndpoint] - Retrieves a list of all registered users with minimal data. Admin endpoint.
        /// - Returns 200 OK with a list of users if any exist.
        /// - Returns 204 No Content if no users are found.
        /// - Returns 500 Internal Server Error for unexpected issues.
        /// Use this endpoint to get a list of all users as an admin.
        /// </summary>
        [HttpGet]
        [AdminEndpoint]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseResult<List<ListUserMinimalGetDto>>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResponseResult<object?>))]
        [Route("User/all")]
        public async Task<ActionResult> GetAllUsers()
        {
            var ct = CancellationTokenSource
                .CreateLinkedTokenSource(HttpContext.RequestAborted, _hostApplicationLifetime.ApplicationStopping)
                .Token;

            var users =
                await _listUserService.GetAllUsersAsync(ct);

            if (users.Count == 0)
                return NoContent();

            return Ok(new ResponseResult<List<ListUserMinimalGetDto>>(users,
                $"{users.Count} users found and retrieved."));
        }

        /// <summary>
        /// [AdminEndpoint] - Removes all data of a user from the database by their user ID. Admin endpoint.
        /// - Returns 200 OK with the number of records deleted if successful.
        /// - Returns 404 Not Found if the user does not exist.
        /// - Returns 500 Internal Server Error for unexpected issues.
        /// Use this endpoint to delete a user as an admin.
        /// </summary>
        /// <param name="userId">The ID of the user to delete.</param>
        [HttpDelete]
        [AdminEndpoint]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseResult<int>))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ResponseResult<Guid>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResponseResult<object?>))]
        [Route("{userId:guid}/admin")]
        public async Task<ActionResult> RemoveUserAsAdmin([FromRoute] Guid userId)
        {
            var ct = CancellationTokenSource
                .CreateLinkedTokenSource(HttpContext.RequestAborted, _hostApplicationLifetime.ApplicationStopping)
                .Token;

            var result = await _listUserService.CheckExistenceAndDeleteUserAsAppAdminAsync(userId, ct);

            if (result.Success is not true)
            {
                if (result.TargetExists is not true)
                    return NotFound(new ResponseResult<Guid>(userId,
                        "User with the provided user id was not found."));

                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ResponseResult<object?>(null,
                        "Due to an internal error, your request could not be processed."));
            }

            return Ok(new ResponseResult<int>(result.RecordsAffected,
                "User successfully deleted. The amount of removed records is returned in the response body."));
        }
    }
}