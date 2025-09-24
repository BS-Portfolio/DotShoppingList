using Microsoft.AspNetCore.Mvc;
using ShoppingListApi.Configs;
using ShoppingListApi.Enums;
using ShoppingListApi.Interfaces.Services;
using ShoppingListApi.Model.DTOs.Get;
using ShoppingListApi.Model.DTOs.Patch;
using ShoppingListApi.Model.DTOs.Post;
using ShoppingListApi.Model.Entity;
using ShoppingListApi.Model.ReturnTypes;

namespace ShoppingListApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ApplicationController(
        IShoppingListService shoppingListService,
        IItemService itemService,
        IListMembershipService listMembershipService,
        IConfiguration configuration,
        IHostApplicationLifetime hostApplicationLifetime,
        ILogger<ApplicationController> logger) : ControllerBase
    {
        private readonly IShoppingListService _shoppingListService = shoppingListService;
        private readonly IItemService _itemService = itemService;
        private readonly IListMembershipService _listMembershipService = listMembershipService;
        private readonly IConfiguration _configuration = configuration;
        private readonly IHostApplicationLifetime _hostApplicationLifetime = hostApplicationLifetime;
        private readonly ILogger<ApplicationController> _logger = logger;

        /// <summary>
        /// [UserEndpoint] - Retrieves all shopping lists for a user (list owner).
        /// Use this endpoint to get all shopping lists for a user by their user ID.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseResult<List<ShoppingListGetDto>>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResponseResult<object?>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(AuthenticationErrorResponse))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(AuthenticationErrorResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResponseResult<object?>))]
        [Route("ShoppingList/all")]
        public async Task<IActionResult> GetShoppingListsForUser()
        {
            var checkAccessResult = this.CheckAccess();

            if (checkAccessResult.ActionResult is not null)
                return checkAccessResult.ActionResult;

            var requestingUserId = checkAccessResult.RequestingUserId!.Value;

            var ct = CancellationTokenSource
                .CreateLinkedTokenSource(_hostApplicationLifetime.ApplicationStopping, HttpContext.RequestAborted)
                .Token;

            var result =
                await _shoppingListService.CheckAccessAndGetAllShoppingListsForUser(requestingUserId,
                    ct);

            if (result.AccessGranted is false)
                return StatusCode(StatusCodes.Status403Forbidden,
                    new AuthenticationErrorResponse(AuthorizationErrorEnum.ListAccessNotGranted));

            if (result.RecordExists is not true || result.Record is null)
                return NoContent();

            return Ok(new ResponseResult<List<ShoppingListGetDto>>(result.Record,
                $"{result.Record.Count} shopping lists found and retrieved."));
        }

        /// <summary>
        /// [UserEndpoint] - Retrieves a single shopping list by its ID for a user (list owner).
        /// Use this endpoint to get a shopping list by its ID and user ID.
        /// </summary>
        /// <param name="shoppingListId">The ID of the shopping list.</param>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResponseResult<object?>))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(Dictionary<string, Guid>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(AuthenticationErrorResponse))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(AuthenticationErrorResponse))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseResult<ShoppingListGetDto>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResponseResult<object?>))]
        [Route("ShoppingList/{shoppingListId:Guid}")]
        public async Task<IActionResult> GetShoppingListForUser(Guid shoppingListId)
        {
            var checkAccessResult = this.CheckAccess();

            if (checkAccessResult.ActionResult is not null)
                return checkAccessResult.ActionResult;

            var requestingUserId = checkAccessResult.RequestingUserId!.Value;

            var ct = CancellationTokenSource
                .CreateLinkedTokenSource(_hostApplicationLifetime.ApplicationStopping, HttpContext.RequestAborted)
                .Token;

            var result =
                await _shoppingListService.CheckAccessAndGetShoppingListByIdAsync(requestingUserId, shoppingListId, ct);

            if (result.AccessGranted is false)
                return StatusCode(StatusCodes.Status403Forbidden,
                    new AuthenticationErrorResponse(AuthorizationErrorEnum.ListAccessNotGranted));

            if (result.RecordExists is false || result.Record is null)
            {
                return NotFound(
                    new ResponseResult<Dictionary<string, Guid>>(new Dictionary<string, Guid>()
                        {
                            { "userId", requestingUserId },
                            { nameof(shoppingListId), shoppingListId }
                        },
                        "Shopping list for the provided id's was not found."
                    ));
            }

            return Ok(new ResponseResult<ShoppingListGetDto>(result.Record, "Shopping list found and retrieved."));
        }

        /// <summary>
        /// [UserEndpoint] - Adds a new shopping list for a user (list owner).
        /// Use this endpoint to add a new shopping list for a user by their user ID.
        /// </summary>
        /// <param name="shoppingListPostDto">The shopping list details.</param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ResponseResult<Guid>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResponseResult<int>))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(AuthenticationErrorResponse))]
        [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ResponseResult<string>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError,
            Type = typeof(ResponseResult<ShoppingListPostDto>))]
        [Route("ShoppingList")]
        public async Task<ActionResult> AddShoppingListForUser(
            [FromBody] ShoppingListPostDto shoppingListPostDto)
        {
            var checkAccessResult = this.CheckAccess();

            if (checkAccessResult.ActionResult is not null)
                return checkAccessResult.ActionResult;

            var requestingUserId = checkAccessResult.RequestingUserId!.Value;

            var ct = CancellationTokenSource
                .CreateLinkedTokenSource(_hostApplicationLifetime.ApplicationStopping, HttpContext.RequestAborted)
                .Token;

            var maxNumberOfListsPerUser =
                _configuration.GetValue<int>("ShoppingLists_MaxAmount");

            if (maxNumberOfListsPerUser < 1)
                maxNumberOfListsPerUser = 5;

            var result =
                await _shoppingListService.CheckConflictAndCreateShoppingListAsync(requestingUserId,
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
                new { shoppingListId = result.AddedShoppingListId },
                new ResponseResult<Guid>(result.AddedShoppingListId.Value,
                    "Shopping list was successfully added for the user."));
        }

        /// <summary>
        /// [UserEndpoint] - Adds a new item to a shopping list.
        /// Use this endpoint to add a new item to a shopping list by its ID.
        /// </summary>
        /// <param name="shoppingListId">The ID of the shopping list.</param>
        /// <param name="itemPostDto">The item details.</param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ResponseResult<Guid>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResponseResult<int>))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(AuthenticationErrorResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ResponseResult<Guid>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResponseResult<object?>))]
        [Route("ShoppingList/{shoppingListId:Guid}/Item")]
        public async Task<ActionResult> AddItemToShoppingList([FromRoute] Guid shoppingListId,
            [FromBody] ItemPostDto itemPostDto)
        {
            var checkAccessResult = this.CheckAccess();

            if (checkAccessResult.ActionResult is not null)
                return checkAccessResult.ActionResult;

            var requestingUserId = checkAccessResult.RequestingUserId!.Value;

            var ct = CancellationTokenSource
                .CreateLinkedTokenSource(_hostApplicationLifetime.ApplicationStopping, HttpContext.RequestAborted)
                .Token;

            var maxItemsAmount = _configuration.GetValue<int>("Items_MaxAmount");

            if (maxItemsAmount <= 0) maxItemsAmount = 20;

            var result = await _itemService.FindShoppingListAndAddItemAsync(requestingUserId,
                shoppingListId, itemPostDto, ct);

            if (result.Success is not true || result.ItemId is null)
            {
                if (result.AccessGranted is false)
                    return StatusCode(StatusCodes.Status403Forbidden,
                        new AuthenticationErrorResponse(AuthorizationErrorEnum.ListAccessNotGranted));

                if (result.ShoppingListExists is false)
                    return NotFound(new ResponseResult<Guid>(shoppingListId,
                        "Shopping list with the provided id was not found."));

                if (result.MaxAmountReached is true)
                    return BadRequest(new ResponseResult<int>(maxItemsAmount,
                        "You have reached the maximum number of allowed items per shopping list. To add a new item, please delete an existing one first."));

                return StatusCode(500,
                    new ResponseResult<object?>(null,
                        "Due to an internal error your request could not be processed."));
            }

            return StatusCode(StatusCodes.Status201Created,
                new ResponseResult<Guid>(result.ItemId.Value, "Item was successfully added to the shopping list."));
        }

        /// <summary>
        /// [UserEndpoint] - Adds a collaborator to a shopping list by the list owner.
        /// Use this endpoint to add a collaborator to a shopping list by the list owner's user ID.
        /// </summary>
        /// <param name="shoppingListId">The ID of the shopping list.</param>
        /// <param name="collaboratorEmailAddress">The email address of the collaborator to add.</param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ResponseResult<string>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResponseResult<string>))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ResponseResult<Guid>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(AuthenticationErrorResponse))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(AuthenticationErrorResponse))]
        [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ResponseResult<string>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResponseResult<object?>))]
        [Route("ShoppingList/{shoppingListId:Guid}/Collaborator/{collaboratorEmailAddress}")]
        public async Task<ActionResult> AddCollaboratorToShoppingList(Guid shoppingListId,
            string collaboratorEmailAddress)
        {
            var checkAccessResult = this.CheckAccess();

            if (checkAccessResult.ActionResult is not null)
                return checkAccessResult.ActionResult;

            var requestingUserId = checkAccessResult.RequestingUserId!.Value;

            var ct = CancellationTokenSource
                .CreateLinkedTokenSource(_hostApplicationLifetime.ApplicationStopping, HttpContext.RequestAborted)
                .Token;

            var result =
                await _listMembershipService.AddCollaboratorToShoppingListAsListOwnerAsync(requestingUserId,
                    collaboratorEmailAddress, shoppingListId, ct);

            if (result.Success is not true)
            {
                if (result.ShoppingListExists is false)
                    return NotFound(new ResponseResult<Guid>(shoppingListId,
                        "Shopping list with the provided id was not found."));

                if (result.RequestingUserIsListOwner is false)
                    return StatusCode(StatusCodes.Status403Forbidden,
                        new AuthenticationErrorResponse(AuthorizationErrorEnum.ListAccessNotGranted));

                if (result.UserRoleIdNotFound is true)
                {
                    _logger.LogError(
                        "Critical error: The user role ID for 'collaborator' was not found in the database. This indicates a misconfiguration of the application.");

                    return StatusCode(500,
                        new ResponseResult<object?>(null,
                            "Due to an internal error, your request could not be processed."));
                }

                if (result.CollaboratorIsRegistered is false)
                    return NotFound(new ResponseResult<string>(collaboratorEmailAddress,
                        "The user with the provided email address was not found."));

                if (result.CollaboratorIsAlreadyAdded is true)
                    return Conflict(new ResponseResult<string>(collaboratorEmailAddress,
                        "The user with the provided email address is already a collaborator of this shopping list."));

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
        /// Use this endpoint to update the name of a shopping list by its ID.
        /// </summary>
        /// <param name="shoppingListId">The ID of the shopping list.</param>
        /// <param name="shoppingListPostDto">The new shopping list details.</param>
        /// <returns></returns>
        [HttpPatch]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseResult<string>))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(AuthenticationErrorResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ResponseResult<Guid>))]
        [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ResponseResult<ShoppingList?>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError,
            Type = typeof(ResponseResult<ShoppingListPostDto>))]
        [Route("ShoppingList/{shoppingListId:Guid}")]
        public async Task<ActionResult> ModifyShoppingListName([FromRoute] Guid shoppingListId,
            [FromBody] ShoppingListPostDto shoppingListPostDto)
        {
            var checkAccessResult = this.CheckAccess();

            if (checkAccessResult.ActionResult is not null)
                return checkAccessResult.ActionResult;

            var requestingUserId = checkAccessResult.RequestingUserId!.Value;

            var ct = CancellationTokenSource
                .CreateLinkedTokenSource(_hostApplicationLifetime.ApplicationStopping, HttpContext.RequestAborted)
                .Token;

            var result = await _shoppingListService.CheckAccessAndUpdateShoppingListNameAsync(requestingUserId,
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
        /// Use this endpoint to update the details of an item in a shopping list by its ID.
        /// </summary>
        /// <param name="shoppingListId">The ID of the shopping list.</param>
        /// <param name="itemId">The ID of the item to update.</param>
        /// <param name="itemPatchDto">The new item details.</param>
        /// <returns></returns>
        [HttpPatch]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseResult<ItemPatchDto>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResponseResult<ItemPatchDto>))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(AuthenticationErrorResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ResponseResult<Dictionary<string, Guid>>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResponseResult<object?>))]
        [Route("ShoppingList/{shoppingListId}/Item/{itemId:Guid}")]
        public async Task<ActionResult> ModifyItemDetails(Guid shoppingListId, Guid itemId,
            ItemPatchDto itemPatchDto)
        {
            var checkAccessResult = this.CheckAccess();

            if (checkAccessResult.ActionResult is not null)
                return checkAccessResult.ActionResult;

            var requestingUserId = checkAccessResult.RequestingUserId!.Value;

            if (itemPatchDto.ItemName is null && itemPatchDto.ItemAmount is null)
                return BadRequest(
                    new ResponseResult<ItemPatchDto>(itemPatchDto, "No valid fields to update were provided."));

            var ct = CancellationTokenSource
                .CreateLinkedTokenSource(_hostApplicationLifetime.ApplicationStopping, HttpContext.RequestAborted)
                .Token;

            var result =
                await _itemService.FindItemAndUpdateAsync(requestingUserId, shoppingListId, itemId, itemPatchDto,
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
        /// Use this endpoint to delete a shopping list by its ID.
        /// </summary>
        /// <param name="shoppingListId">The ID of the shopping list to delete.</param>
        /// <returns></returns>
        [HttpDelete]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResponseResult<object?>))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseResult<int>))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(AuthenticationErrorResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ResponseResult<Guid>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResponseResult<object?>))]
        [Route("ShoppingList/{shoppingListId:Guid}")]
        public async Task<ActionResult> RemoveShoppingList(Guid shoppingListId)
        {
            var checkAccessResult = this.CheckAccess();

            if (checkAccessResult.ActionResult is not null)
                return checkAccessResult.ActionResult;

            var requestingUserId = checkAccessResult.RequestingUserId!.Value;

            var ct = CancellationTokenSource
                .CreateLinkedTokenSource(_hostApplicationLifetime.ApplicationStopping, HttpContext.RequestAborted)
                .Token;

            var result = await _shoppingListService.CheckAccessAndDeleteShoppingListAsync(
                requestingUserId, shoppingListId, ct);

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
                "Shopping list was successfully deleted. Amount of deleted records is attached."));
        }

        /// <summary>
        /// [UserEndpoint] - Removes an item from a shopping list.
        /// Use this endpoint to delete an item from a shopping list by its ID.
        /// </summary>
        /// <param name="shoppingListId">The ID of the shopping list.</param>
        /// <param name="itemId">The ID of the item to delete.</param>
        /// <returns></returns>
        [HttpDelete]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResponseResult<object?>))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseResult<Guid>))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(AuthenticationErrorResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ResponseResult<object?>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResponseResult<object?>))]
        [Route(("ShoppingList/{shoppingListId:Guid}/Item/{itemId:Guid}"))]
        public async Task<ActionResult> RemoveItemFromShoppingList(Guid shoppingListId, Guid itemId)
        {
            var checkAccessResult = this.CheckAccess();

            if (checkAccessResult.ActionResult is not null)
                return checkAccessResult.ActionResult;

            var requestingUserId = checkAccessResult.RequestingUserId!.Value;

            var ct = CancellationTokenSource
                .CreateLinkedTokenSource(_hostApplicationLifetime.ApplicationStopping, HttpContext.RequestAborted)
                .Token;

            var result = await _itemService.FindItemAndDeleteAsync(requestingUserId, shoppingListId, itemId, ct);

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

            return Ok(new ResponseResult<int>(result.RecordsAffected,
                "Item was successfully deleted from the shopping list. Amount of deleted records is attached."));
        }

        /// <summary>
        /// [UserEndpoint] - Removes a collaborator from a shopping list. Either the list owner removes a collaborator or a collaborator leaves the list.
        /// Use this endpoint to remove a collaborator from a shopping list by the list owner's user ID.
        /// </summary>
        /// <param name="shoppingListId">The ID of the shopping list.</param>
        /// <param name="collaboratorId">The ID of the collaborator to remove.</param>
        /// <returns></returns>
        [HttpDelete]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseResult<string>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResponseResult<string>))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(AuthenticationErrorResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ResponseResult<Guid>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResponseResult<object?>))]
        [Route("ShoppingList/{shoppingListId:Guid}/Collaborator/{collaboratorId:Guid}/kick")]
        public async Task<ActionResult> KickCollaboratorFromShoppingList(
            [FromRoute] Guid shoppingListId, [FromRoute] Guid collaboratorId)
        {
            var checkAccessResult = this.CheckAccess();

            if (checkAccessResult.ActionResult is not null)
                return checkAccessResult.ActionResult;

            var requestingUserId = checkAccessResult.RequestingUserId!.Value;

            var ct = CancellationTokenSource
                .CreateLinkedTokenSource(_hostApplicationLifetime.ApplicationStopping, HttpContext.RequestAborted)
                .Token;

            var result =
                await _listMembershipService.RemoveCollaboratorFromShoppingListAsListOwnerAsync(
                    requestingUserId, collaboratorId, shoppingListId, ct);


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

        /// <summary>
        /// [UserEndpoint] - Removes a collaborator from a shopping list. Either the list owner kicks a collaborator or a collaborator leaves the list.
        /// Use this endpoint to remove a collaborator from a shopping list by the list owner's user ID.
        /// </summary>
        /// <param name="shoppingListId">The ID of the shopping list.</param>
        /// <param name="collaboratorId">The ID of the collaborator to remove.</param>
        /// <returns></returns>
        [HttpDelete]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseResult<string>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResponseResult<string>))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(AuthenticationErrorResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ResponseResult<Guid>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResponseResult<object?>))]
        [Route("ShoppingList/{shoppingListId:Guid}/Collaborator/{collaboratorId:Guid}/leave")]
        public async Task<ActionResult> LeaveShoppingListAsCollaborator(
            [FromRoute] Guid shoppingListId, [FromRoute] Guid collaboratorId)
        {
            var checkAccessResult = this.CheckAccess();

            if (checkAccessResult.ActionResult is not null)
                return checkAccessResult.ActionResult;

            var requestingUserId = checkAccessResult.RequestingUserId!.Value;

            var ct = CancellationTokenSource
                .CreateLinkedTokenSource(_hostApplicationLifetime.ApplicationStopping, HttpContext.RequestAborted)
                .Token;

            var result =
                await _listMembershipService.LeaveShoppingListAsCollaboratorAsync(requestingUserId,
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