using Microsoft.AspNetCore.Mvc;
using ShoppingListApi.Enums;
using ShoppingListApi.Interfaces.Services;
using ShoppingListApi.Model.DTOs.Get;
using ShoppingListApi.Model.DTOs.Patch;
using ShoppingListApi.Model.DTOs.Post;
using ShoppingListApi.Model.Entity;
using ShoppingListApi.Model.ReturnTypes;

namespace ShoppingListApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApplicationController(
        IListUserService listUserService,
        IShoppingListService shoppingListService,
        IItemService itemService,
        IListMembershipService listMembershipService,
        IConfiguration configuration,
        IHostApplicationLifetime hostApplicationLifetime,
        ILogger<ApplicationController> logger) : ControllerBase
    {
        private readonly IListUserService _listUserService = listUserService;
        private readonly IShoppingListService _shoppingListService = shoppingListService;
        private readonly IItemService _itemService = itemService;
        private readonly IListMembershipService _listMembershipService = listMembershipService;
        private readonly IConfiguration _configuration = configuration;
        private readonly IHostApplicationLifetime _hostApplicationLifetime = hostApplicationLifetime;
        private readonly ILogger<ApplicationController> _logger = logger;

        /// <summary>
        /// [UserEndpoint] - Retrieves all shopping lists for a user (list owner).
        /// - Returns 200 OK with a list of shopping lists if any exist.
        /// - Returns 204 No Content if no shopping lists are found.
        /// - Returns 401 Unauthorized if the user credentials are missing or access is not granted.
        /// - Returns 500 Internal Server Error for unexpected issues.
        /// Use this endpoint to get all shopping lists for a user by their user ID.
        /// </summary>
        /// <param name="userId">The ID of the list owner.</param>
        /// <param name="requestingUserId">The ID of the user making the request (from header).</param>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseResult<List<ShoppingListGetDto>>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(AuthenticationErrorResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResponseResult<object?>))]
        [Route("User/{userId:Guid}/ShoppingList/all")]
        public async Task<ActionResult> GetShoppingListsForUser([FromRoute] Guid userId,
            [FromHeader(Name = "USER-ID")] Guid? requestingUserId)
        {
            if (requestingUserId is null)
                return Unauthorized(new AuthenticationErrorResponse(AuthorizationErrorEnum.UserCredentialsMissing));

            if (!requestingUserId.Equals(userId))
                return Unauthorized(new AuthenticationErrorResponse(AuthorizationErrorEnum.ListAccessNotGranted));

            var ct = CancellationTokenSource
                .CreateLinkedTokenSource(_hostApplicationLifetime.ApplicationStopping, HttpContext.RequestAborted)
                .Token;

            var result =
                await _shoppingListService.CheckAccessAndGetAllShoppingListsForUser(requestingUserId.Value, userId,
                    ct);

            if (result.AccessGranted is false)
                return Unauthorized(new AuthenticationErrorResponse(AuthorizationErrorEnum.ListAccessNotGranted));

            if (result.RecordExists is not true || result.Record is null)
                return NoContent();

            return Ok(new ResponseResult<List<ShoppingListGetDto>>(result.Record,
                $"{result.Record.Count} shopping lists found and retrieved."));
        }

        /// <summary>
        /// [UserEndpoint] - Retrieves a single shopping list by its ID for a user (list owner).
        /// - Returns 200 OK with the shopping list if found.
        /// - Returns 404 Not Found if the shopping list does not exist.
        /// - Returns 401 Unauthorized if the user credentials are missing or access is not granted.
        /// - Returns 500 Internal Server Error for unexpected issues.
        /// Use this endpoint to get a shopping list by its ID and user ID.
        /// </summary>
        /// <param name="userId">The ID of the list owner.</param>
        /// <param name="shoppingListId">The ID of the shopping list.</param>
        /// <param name="requestingUserId">The ID of the user making the request (from header).</param>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ResponseResult<Dictionary<string, Guid>>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(AuthenticationErrorResponse))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseResult<ShoppingListGetDto>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResponseResult<object?>))]
        [Route("User/{userId:Guid}/ShoppingList/{shoppingListId:Guid}")]
        public async Task<ActionResult> GetShoppingListForUser([FromRoute] Guid userId, [FromRoute] Guid shoppingListId,
            [FromHeader(Name = "USER-ID")] Guid? requestingUserId)
        {
            if (requestingUserId is null)
                return Unauthorized(new AuthenticationErrorResponse(AuthorizationErrorEnum.UserCredentialsMissing));

            if (!requestingUserId.Equals(userId))
                return Unauthorized(new AuthenticationErrorResponse(AuthorizationErrorEnum.ListAccessNotGranted));

            var ct = CancellationTokenSource
                .CreateLinkedTokenSource(_hostApplicationLifetime.ApplicationStopping, HttpContext.RequestAborted)
                .Token;

            var result =
                await _shoppingListService.CheckAccessAndGetShoppingListByIdAsync(requestingUserId.Value, userId,
                    shoppingListId, ct);

            if (result.AccessGranted is false)
            {
                return Unauthorized(
                    new AuthenticationErrorResponse(AuthorizationErrorEnum.ListAccessNotGranted));
            }

            if (result.RecordExists is false || result.Record is null)
            {
                return NotFound(
                    new ResponseResult<Dictionary<string, Guid>>(new Dictionary<string, Guid>()
                        {
                            { nameof(userId), userId },
                            { nameof(shoppingListId), shoppingListId }
                        },
                        "Shopping list for the provided id's was not found."
                    ));
            }

            return Ok(new ResponseResult<ShoppingListGetDto>(result.Record, "Shopping list found and retrieved."));
        }

        /// <summary>
        /// [UserEndpoint] - Adds a new shopping list for a user (list owner).
        /// - Returns 201 Created with the new shopping list ID if successful.
        /// - Returns 400 Bad Request if the maximum number of lists is reached.
        /// - Returns 403 Forbidden if access is not granted.
        /// - Returns 409 Conflict if a list with the same name exists.
        /// - Returns 500 Internal Server Error for unexpected issues.
        /// Use this endpoint to add a new shopping list for a user by their user ID.
        /// </summary>
        /// <param name="userId">The ID of the list owner.</param>
        /// <param name="shoppingListPostDto">The shopping list details.</param>
        /// <param name="requestingUserId">The ID of the user making the request (from header).</param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ResponseResult<Guid>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResponseResult<int>))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(AuthenticationErrorResponse))]
        [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ResponseResult<string>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError,
            Type = typeof(ResponseResult<ShoppingListPostDto>))]
        [Route("User/{userId:Guid}/ShoppingList")]
        public async Task<ActionResult> AddShoppingListForUser([FromRoute] Guid userId,
            [FromBody] ShoppingListPostDto shoppingListPostDto,
            [FromHeader(Name = "USER-ID")] Guid? requestingUserId)
        {
            if (requestingUserId is null)
                return Unauthorized(new AuthenticationErrorResponse(AuthorizationErrorEnum.UserCredentialsMissing));

            if (!requestingUserId.Equals(userId))
                return Unauthorized(new AuthenticationErrorResponse(AuthorizationErrorEnum.ListAccessNotGranted));

            var ct = CancellationTokenSource
                .CreateLinkedTokenSource(_hostApplicationLifetime.ApplicationStopping, HttpContext.RequestAborted)
                .Token;

            var maxNumberOfListsPerUser =
                _configuration.GetValue<int>("ShoppingLists_MaxAmount");

            if (maxNumberOfListsPerUser < 1)
                maxNumberOfListsPerUser = 5;

            var result =
                await _shoppingListService.CheckConflictAndCreateShoppingListAsync(requestingUserId.Value, userId,
                    shoppingListPostDto, ct);

            if (result.AccessGranted is not true)
                return StatusCode(StatusCodes.Status403Forbidden,
                    new AuthenticationErrorResponse(AuthorizationErrorEnum.ListAccessNotGranted));

            if (result.Success is not true)
            {
                if (result.MaximumNumberOfListsReached is true)
                {
                    return BadRequest(new ResponseResult<int>(maxNumberOfListsPerUser,
                        "You have reached the maximum number of allowed shopping lists per user. To add a new list, please delete an existing one first."));
                }

                if (result.NameAlreadyExists is true)
                {
                    return Conflict(new ResponseResult<string>(shoppingListPostDto.ShoppingListName,
                        "A shopping list with the same name already exists for this user. Please choose a different name."));
                }

                if (result.ListAssignmentSuccess is false)
                {
                    return StatusCode(500,
                        new ResponseResult<ShoppingListPostDto>(shoppingListPostDto,
                            "Failed to add the shopping list for the user due to an internal error."));
                }

                return StatusCode(500, new ResponseResult<ShoppingListPostDto>(shoppingListPostDto,
                    "Due to an internal error your request could not be processed."));
            }

            if (result.AddedShoppingListId is null)
            {
                return StatusCode(500, new ResponseResult<ShoppingListPostDto>(shoppingListPostDto,
                    "Due to an internal error your request could not be processed."));
            }

            return CreatedAtAction(nameof(GetShoppingListForUser),
                new { userId, shoppingListId = result.AddedShoppingListId },
                new ResponseResult<Guid>(result.AddedShoppingListId.Value,
                    "Shopping list was successfully added for the user."));
        }

        /// <summary>
        /// [UserEndpoint] - Adds a new item to a shopping list.
        /// - Returns 200 OK with the new item ID if successful.
        /// - Returns 400 Bad Request if the maximum number of items is reached.
        /// - Returns 403 Forbidden if access is not granted.
        /// - Returns 404 Not Found if the shopping list does not exist.
        /// - Returns 500 Internal Server Error for unexpected issues.
        /// Use this endpoint to add a new item to a shopping list by its ID.
        /// </summary>
        /// <param name="userId">The ID of the list owner.</param>
        /// <param name="shoppingListId">The ID of the shopping list.</param>
        /// <param name="itemPostDto">The item details.</param>
        /// <param name="requestingUserId">The ID of the user making the request (from header).</param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseResult<Guid>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResponseResult<int>))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(AuthenticationErrorResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ResponseResult<Guid>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResponseResult<object?>))]
        [Route("User/{userId:Guid}/ShoppingList/{shoppingListId:Guid}/Item")]
        public async Task<ActionResult> AddItemToShoppingList([FromRoute] Guid userId, [FromRoute] Guid shoppingListId,
            [FromBody] ItemPostDto itemPostDto, [FromHeader(Name = "USER-ID")] Guid? requestingUserId)
        {
            if (requestingUserId is null)
                return Unauthorized(new AuthenticationErrorResponse(AuthorizationErrorEnum.UserCredentialsMissing));

            if (!requestingUserId.Equals(userId))
                return Unauthorized(new AuthenticationErrorResponse(AuthorizationErrorEnum.ListAccessNotGranted));

            var ct = CancellationTokenSource
                .CreateLinkedTokenSource(_hostApplicationLifetime.ApplicationStopping, HttpContext.RequestAborted)
                .Token;

            var maxItemsAmount = _configuration.GetValue<int>("Items_MaxAmount");

            if (maxItemsAmount <= 0) maxItemsAmount = 20;

            var result = await _itemService.FindShoppingListAndAddItemAsync(requestingUserId.Value,
                shoppingListId, itemPostDto, ct);

            if (result.Success is not true || result.ItemId is null)
            {
                if (result.AccessGranted is false)
                {
                    return StatusCode(StatusCodes.Status403Forbidden,
                        new AuthenticationErrorResponse(AuthorizationErrorEnum.ListAccessNotGranted));
                }

                if (result.ShoppingListExists is false)
                    return NotFound(new ResponseResult<Guid>(shoppingListId,
                        "Shopping list with the provided id was not found."));

                if (result.MaxAmountReached is true)
                {
                    return BadRequest(new ResponseResult<int>(maxItemsAmount,
                        "You have reached the maximum number of allowed items per shopping list. To add a new item, please delete an existing one first."));
                }

                return StatusCode(500,
                    new ResponseResult<object?>(null,
                        "Due to an internal error your request could not be processed."));
            }

            return Ok(result.ItemId);
        }

        /// <summary>
        /// [UserEndpoint] - Adds a collaborator to a shopping list by the list owner.
        /// - Returns 201 Created if the collaborator was added successfully.
        /// - Returns 400 Bad Request if the collaborator is not found.
        /// - Returns 401 Unauthorized if the user credentials are missing or access is not granted.
        /// - Returns 404 Not Found if the shopping list or collaborator does not exist.
        /// - Returns 409 Conflict if the collaborator is already added.
        /// - Returns 500 Internal Server Error for unexpected issues.
        /// Use this endpoint to add a collaborator to a shopping list by the list owner's user ID.
        /// </summary>
        /// <param name="requestingUserId">The ID of the user making the request (from header).</param>
        /// <param name="userId">The ID of the list owner.</param>
        /// <param name="shoppingListId">The ID of the shopping list.</param>
        /// <param name="collaboratorEmailAddress">The email address of the collaborator to add.</param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ResponseResult<string>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResponseResult<string>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(AuthenticationErrorResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ResponseResult<Guid>))]
        [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ResponseResult<string>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResponseResult<object?>))]
        [Route("User/{userId:Guid}/ShoppingList/{shoppingListId:Guid}/Collaborator/{collaboratorEmailAddress}")]
        public async Task<ActionResult> AddCollaboratorToShoppingList(
            [FromHeader(Name = "USER-ID")] Guid? requestingUserId,
            Guid userId, Guid shoppingListId, string collaboratorEmailAddress)
        {
            if (requestingUserId is null)
                return Unauthorized(new AuthenticationErrorResponse(AuthorizationErrorEnum.UserCredentialsMissing));

            if (requestingUserId != userId)
                return Unauthorized(new AuthenticationErrorResponse(AuthorizationErrorEnum.ListAccessNotGranted));

            var ct = CancellationTokenSource
                .CreateLinkedTokenSource(_hostApplicationLifetime.ApplicationStopping, HttpContext.RequestAborted)
                .Token;

            var result =
                await _listMembershipService.AddCollaboratorToShoppingListAsListOwnerAsync(requestingUserId.Value,
                    collaboratorEmailAddress, shoppingListId, ct);

            if (result.Success is not true)
            {
                if (result.ShoppingListExists is false)
                {
                    return NotFound(new ResponseResult<Guid>(shoppingListId,
                        "Shopping list with the provided id was not found."));
                }

                if (result.RequestingUserIsListOwner is false)
                {
                    return Unauthorized(
                        new AuthenticationErrorResponse(AuthorizationErrorEnum.ListAccessNotGranted));
                }

                if (result.UserRoleIdNotFound is true)
                {
                    _logger.LogError(
                        "Critical error: The user role ID for 'collaborator' was not found in the database. This indicates a misconfiguration of the application.");

                    return StatusCode(500,
                        new ResponseResult<object?>(null,
                            "Due to an internal error, your request could not be processed."));
                }

                if (result.CollaboratorIsRegistered is false)
                {
                    return NotFound(new ResponseResult<string>(collaboratorEmailAddress,
                        "The user with the provided email address was not found."));
                }

                if (result.CollaboratorIsAlreadyAdded is true)
                {
                    return Conflict(new ResponseResult<string>(collaboratorEmailAddress,
                        "The user with the provided email address is already a collaborator of this shopping list."));
                }

                return StatusCode(500,
                    new ResponseResult<object?>(null,
                        "Due to an internal error, your request could not be processed."));
            }

            return StatusCode(201,
                new ResponseResult<string>(collaboratorEmailAddress,
                    "User with the provided email address was added to your shopping list."));
        }

        /// <summary>
        /// [UserEndpoint] - Modifies the name of a shopping list by the list owner.
        /// - Returns 200 OK if the name was updated successfully.
        /// - Returns 403 Forbidden if access is not granted.
        /// - Returns 404 Not Found if the shopping list does not exist.
        /// - Returns 409 Conflict if the new name would cause a conflict.
        /// - Returns 500 Internal Server Error for unexpected issues.
        /// Use this endpoint to update the name of a shopping list by its ID.
        /// </summary>
        /// <param name="userId">The ID of the list owner.</param>
        /// <param name="shoppingListId">The ID of the shopping list.</param>
        /// <param name="shoppingListPostDto">The new shopping list details.</param>
        /// <param name="requestingUserId">The ID of the user making the request (from header).</param>
        /// <returns></returns>
        [HttpPatch]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseResult<string>))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(AuthenticationErrorResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ResponseResult<Guid>))]
        [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ResponseResult<ShoppingList?>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError,
            Type = typeof(ResponseResult<ShoppingListPostDto>))]
        [Route("User/{userId:Guid}/ShoppingList/{shoppingListId:Guid}")]
        public async Task<ActionResult> ModifyShoppingListName([FromRoute] Guid userId, [FromRoute] Guid shoppingListId,
            [FromBody] ShoppingListPostDto shoppingListPostDto, [FromHeader(Name = "USER-ID")] Guid? requestingUserId)
        {
            if (requestingUserId is null)
                return Unauthorized(new AuthenticationErrorResponse(AuthorizationErrorEnum.UserCredentialsMissing));

            if (!userId.Equals(requestingUserId))
                return Unauthorized(new AuthenticationErrorResponse(AuthorizationErrorEnum.ListAccessNotGranted));

            var ct = CancellationTokenSource
                .CreateLinkedTokenSource(_hostApplicationLifetime.ApplicationStopping, HttpContext.RequestAborted)
                .Token;

            var result = await _shoppingListService.CheckAccessAndUpdateShoppingListNameAsync(requestingUserId.Value,
                shoppingListId, shoppingListPostDto, ct);

            if (result.Success is not true)
            {
                if (result.AccessGranted is false)
                    return StatusCode(StatusCodes.Status403Forbidden,
                        new AuthenticationErrorResponse(AuthorizationErrorEnum.ListAccessNotGranted));

                if (result.TargetExists is false)
                    return NotFound(new ResponseResult<Guid>(shoppingListId,
                        "Shopping list with the provided id was not found."));

                if (result.Conflicts is true)
                    return Conflict(new ResponseResult<ShoppingList?>(result.ConflictingRecord,
                        "Updating the shopping list name would cause a conflict with an existing shopping list."));

                return StatusCode(500, new ResponseResult<ShoppingListPostDto>(shoppingListPostDto,
                    "Due to an internal error your request could not be processed."));
            }

            return Ok(new ResponseResult<string>(shoppingListPostDto.ShoppingListName,
                "Shopping list name was successfully updated."));
        }

        /// <summary>
        /// [UserEndpoint] - Modifies the details of an item in a shopping list.
        /// - Returns 200 OK if the item was updated successfully.
        /// - Returns 400 Bad Request if no valid fields to update were provided.
        /// - Returns 403 Forbidden if access is not granted.
        /// - Returns 404 Not Found if the item or shopping list does not exist.
        /// - Returns 500 Internal Server Error for unexpected issues.
        /// Use this endpoint to update the details of an item in a shopping list by its ID.
        /// </summary>
        /// <param name="userId">The ID of the list owner.</param>
        /// <param name="shoppingListId">The ID of the shopping list.</param>
        /// <param name="itemId">The ID of the item to update.</param>
        /// <param name="itemPatchDto">The new item details.</param>
        /// <param name="requestingUserId">The ID of the user making the request (from header).</param>
        /// <returns></returns>
        [HttpPatch]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseResult<ItemPatchDto>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResponseResult<ItemPatchDto>))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(AuthenticationErrorResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ResponseResult<Dictionary<string, Guid>>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResponseResult<object?>))]
        [Route("User/{userId:Guid}/ShoppingList/{shoppingListId}/Item/{itemId:Guid}")]
        public async Task<ActionResult> ModifyItemDetails(Guid userId, Guid shoppingListId, Guid itemId,
            ItemPatchDto itemPatchDto, [FromHeader(Name = "USER-ID")] Guid? requestingUserId)
        {
            if (requestingUserId is null)
                return Unauthorized(new AuthenticationErrorResponse(AuthorizationErrorEnum.UserCredentialsMissing));

            if (!userId.Equals(requestingUserId.Value))
                return Unauthorized(new AuthenticationErrorResponse(AuthorizationErrorEnum.ListAccessNotGranted));

            if (itemPatchDto.ItemName is null && itemPatchDto.ItemAmount is null)
                return BadRequest(
                    new ResponseResult<ItemPatchDto>(itemPatchDto, "No valid fields to update were provided."));

            var ct = CancellationTokenSource
                .CreateLinkedTokenSource(_hostApplicationLifetime.ApplicationStopping, HttpContext.RequestAborted)
                .Token;

            var result =
                await _itemService.FindItemAndUpdateAsync(requestingUserId.Value, shoppingListId, itemId, itemPatchDto,
                    ct);

            if (result.Success is not true)
            {
                if (result.AccessGranted is false)
                    return StatusCode(StatusCodes.Status403Forbidden,
                        new AuthenticationErrorResponse(AuthorizationErrorEnum.ListAccessNotGranted));

                if (result.TargetExists is false)
                    return NotFound(new ResponseResult<Dictionary<string, Guid>>(
                        new Dictionary<string, Guid>()
                        {
                            { nameof(shoppingListId), shoppingListId },
                            { nameof(itemId), itemId }
                        },
                        "Shopping list with the provided ids was not found"));

                return StatusCode(500,
                    new ResponseResult<object?>(null, "Due to an internal error your request could not be processed."));
            }

            return Ok(new ResponseResult<ItemPatchDto>(itemPatchDto, "Item details were successfully updated."));
        }

        /// <summary>
        /// [UserEndpoint] - Removes a shopping list for the list owner.
        /// - Returns 200 OK if the shopping list was deleted successfully.
        /// - Returns 403 Forbidden if access is not granted.
        /// - Returns 404 Not Found if the shopping list does not exist.
        /// - Returns 500 Internal Server Error for unexpected issues.
        /// Use this endpoint to delete a shopping list by its ID.
        /// </summary>
        /// <param name="userId">The ID of the list owner.</param>
        /// <param name="shoppingListId">The ID of the shopping list to delete.</param>
        /// <param name="requestingUserId">The ID of the user making the request (from header).</param>
        /// <returns></returns>
        [HttpDelete]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseResult<int>))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(AuthenticationErrorResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ResponseResult<Guid>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResponseResult<object?>))]
        [Route("User/{userId:Guid}/ShoppingList/{shoppingListId:Guid}")]
        public async Task<ActionResult> RemoveShoppingList([FromRoute] Guid userId, [FromRoute] Guid shoppingListId,
            [FromHeader(Name = "USER-ID")] Guid? requestingUserId)
        {
            if (requestingUserId is null)
                return Unauthorized(new AuthenticationErrorResponse(AuthorizationErrorEnum.UserCredentialsMissing));

            if (!requestingUserId.Equals(userId))
                return Unauthorized(new AuthenticationErrorResponse(AuthorizationErrorEnum.ListAccessNotGranted));

            var ct = CancellationTokenSource
                .CreateLinkedTokenSource(_hostApplicationLifetime.ApplicationStopping, HttpContext.RequestAborted)
                .Token;

            var result = await _shoppingListService.CheckAccessAndDeleteShoppingListAsync(
                requestingUserId.Value, shoppingListId, ct);

            if (result.Success is not true)
            {
                if (result.AccessGranted is false)
                    return StatusCode(StatusCodes.Status403Forbidden,
                        new AuthenticationErrorResponse(AuthorizationErrorEnum.ListAccessNotGranted));

                if (result.TargetExists is false)
                    return NotFound(new ResponseResult<Guid>(shoppingListId,
                        "Shopping list with the provided id was not found."));


                return StatusCode(500,
                    new ResponseResult<object?>(null,
                        "Due to an internal error your request could not be processed."));
            }

            return Ok(new ResponseResult<int>(result.RecordsAffected,
                $"Shopping list was successfully deleted. Amount of deleted records is attached."));
        }

        /// <summary>
        /// [UserEndpoint] - Removes an item from a shopping list.
        /// - Returns 200 OK if the item was deleted successfully.
        /// - Returns 403 Forbidden if access is not granted.
        /// - Returns 404 Not Found if the item or shopping list does not exist.
        /// - Returns 500 Internal Server Error for unexpected issues.
        /// Use this endpoint to delete an item from a shopping list by its ID.
        /// </summary>
        /// <param name="userId">The ID of the list owner.</param>
        /// <param name="shoppingListId">The ID of the shopping list.</param>
        /// <param name="itemId">The ID of the item to delete.</param>
        /// <param name="requestingUserId">The ID of the user making the request (from header).</param>
        /// <returns></returns>
        [HttpDelete]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseResult<string>))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(AuthenticationErrorResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ResponseResult<Dictionary<string, Guid>>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResponseResult<object?>))]
        [Route(("User/{userId:Guid}/ShoppingList/{shoppingListId:Guid}/Item/{itemId:Guid}"))]
        public async Task<ActionResult> RemoveItemFromShoppingList([FromRoute] Guid userId,
            [FromRoute] Guid shoppingListId, [FromRoute] Guid itemId,
            [FromHeader(Name = "USER-ID")] Guid? requestingUserId)
        {
            if (requestingUserId is null)
                return Unauthorized(new AuthenticationErrorResponse(AuthorizationErrorEnum.UserCredentialsMissing));

            if (!requestingUserId.Equals(userId))
                return Unauthorized(new AuthenticationErrorResponse(AuthorizationErrorEnum.ListAccessNotGranted));

            var ct = CancellationTokenSource
                .CreateLinkedTokenSource(_hostApplicationLifetime.ApplicationStopping, HttpContext.RequestAborted)
                .Token;

            var result = await _itemService.FindItemAndDeleteAsync(requestingUserId.Value, shoppingListId, itemId, ct);

            if (result.Success is not true)
            {
                if (result.AccessGranted is false)
                    return StatusCode(StatusCodes.Status403Forbidden,
                        new AuthenticationErrorResponse(AuthorizationErrorEnum.ListAccessNotGranted));

                if (result.TargetExists is false)
                    return NotFound(new ResponseResult<Dictionary<string, Guid>>(new Dictionary<string, Guid>()
                        {
                            { nameof(shoppingListId), shoppingListId },
                            { nameof(itemId), itemId }
                        },
                        "Shopping list with the provided ids was not found."));

                return StatusCode(500,
                    new ResponseResult<object?>(null,
                        "Due to an internal error your request could not be processed."));
            }

            return Ok("Successfully removed!");
        }

        /// <summary>
        /// [UserEndpoint] - Removes a collaborator from a shopping list. Either the list owner removes a collaborator or a collaborator leaves the list.
        /// - Returns 200 OK if the collaborator was removed successfully.
        /// - Returns 400 Bad Request if the collaborator is not found.
        /// - Returns 403 Forbidden if access is not granted.
        /// - Returns 404 Not Found if the shopping list or collaborator does not exist.
        /// - Returns 500 Internal Server Error for unexpected issues.
        /// Use this endpoint to remove a collaborator from a shopping list by the list owner's user ID.
        /// </summary>
        /// <param name="requestingUserId">The ID of the user making the request (from header).</param>
        /// <param name="userId">The ID of the list owner.</param>
        /// <param name="shoppingListId">The ID of the shopping list.</param>
        /// <param name="collaboratorId">The ID of the collaborator to remove.</param>
        /// <returns></returns>
        [HttpDelete]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseResult<string>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResponseResult<string>))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(AuthenticationErrorResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ResponseResult<Guid>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResponseResult<object?>))]
        [Route("User/{userId:Guid}/ShoppingList/{shoppingListId:Guid}/Collaborator/{collaboratorId:Guid}/kick")]
        public async Task<ActionResult> KickCollaboratorFromShoppingList(
            [FromHeader(Name = "USER-ID")] Guid? requestingUserId, [FromRoute] Guid userId,
            [FromRoute] Guid shoppingListId, [FromRoute] Guid collaboratorId)
        {
            if (requestingUserId is null)
                return Unauthorized(new AuthenticationErrorResponse(AuthorizationErrorEnum.UserCredentialsMissing));

            if (!requestingUserId.Equals(userId))
                return Unauthorized(new AuthenticationErrorResponse(AuthorizationErrorEnum.ListAccessNotGranted));

            var ct = CancellationTokenSource
                .CreateLinkedTokenSource(_hostApplicationLifetime.ApplicationStopping, HttpContext.RequestAborted)
                .Token;

            var result =
                await _listMembershipService.RemoveCollaboratorFromShoppingListAsListOwnerAsync(
                    requestingUserId.Value, collaboratorId, shoppingListId, ct);


            if (result.Success is not true)
            {
                if (result.AccessGranted is false)
                    return StatusCode(StatusCodes.Status403Forbidden,
                        new AuthenticationErrorResponse(AuthorizationErrorEnum.ListAccessNotGranted));

                if (result.TargetExists is false)
                    return NotFound(new ResponseResult<Guid>(collaboratorId,
                        "No collaborator with the provided id was found for this shopping list."));

                return StatusCode(500,
                    new ResponseResult<object?>(null,
                        "Due to an internal error your request could not be processed."));
            }

            return Ok(new ResponseResult<int>(result.RecordsAffected,
                $"Successfully removed! Amount of deleted records is attached."));
        }

        /// <summary>
        /// [UserEndpoint] - Removes a collaborator from a shopping list. Either the list owner kicks a collaborator or a collaborator leaves the list.
        /// - Returns 200 OK if the collaborator was removed successfully.
        /// - Returns 400 Bad Request if the collaborator is not found.
        /// - Returns 403 Forbidden if access is not granted.
        /// - Returns 404 Not Found if the shopping list or collaborator does not exist.
        /// - Returns 500 Internal Server Error for unexpected issues.
        /// Use this endpoint to remove a collaborator from a shopping list by the list owner's user ID.
        /// </summary>
        /// <param name="requestingUserId">The ID of the user making the request (from header).</param>
        /// <param name="userId">The ID of the list owner.</param>
        /// <param name="shoppingListId">The ID of the shopping list.</param>
        /// <param name="collaboratorId">The ID of the collaborator to remove.</param>
        /// <returns></returns>
        [HttpDelete]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseResult<string>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResponseResult<string>))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(AuthenticationErrorResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ResponseResult<Guid>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResponseResult<object?>))]
        [Route("User/{userId:Guid}/ShoppingList/{shoppingListId:Guid}/Collaborator/{collaboratorId:Guid}/leave")]
        public async Task<ActionResult> LeaveShoppingListAsCollaborator(
            [FromHeader(Name = "USER-ID")] Guid? requestingUserId,
            [FromRoute] Guid userId, [FromRoute] Guid shoppingListId, [FromRoute] Guid collaboratorId)
        {
            if (requestingUserId is null)
                return Unauthorized(new AuthenticationErrorResponse(AuthorizationErrorEnum.UserCredentialsMissing));

            if (!requestingUserId.Equals(collaboratorId))
                return Unauthorized(new AuthenticationErrorResponse(AuthorizationErrorEnum.ListAccessNotGranted));

            var ct = CancellationTokenSource
                .CreateLinkedTokenSource(_hostApplicationLifetime.ApplicationStopping, HttpContext.RequestAborted)
                .Token;

            var result =
                await _listMembershipService.LeaveShoppingListAsCollaboratorAsync(requestingUserId.Value,
                    shoppingListId, collaboratorId, ct);

            if (result.Success is not true)
            {
                if (result.AccessGranted is false)
                    return StatusCode(StatusCodes.Status403Forbidden,
                        new AuthenticationErrorResponse(AuthorizationErrorEnum.ListAccessNotGranted));

                if (result.TargetExists is false)
                    return NotFound(new ResponseResult<Guid>(collaboratorId,
                        "No collaborator with the provided id was found for this shopping list."));

                return StatusCode(500,
                    new ResponseResult<object?>(null,
                        "Due to an internal error your request could not be processed."));
            }

            return Ok(new ResponseResult<int>(result.RecordsAffected,
                "Successfully removed! Amount of deleted records is attached."));
        }
    }
}