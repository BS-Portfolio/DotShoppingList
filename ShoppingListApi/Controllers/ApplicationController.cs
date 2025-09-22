using Microsoft.AspNetCore.Mvc;
using ShoppingListApi.Configs;
using ShoppingListApi.Enums;
using ShoppingListApi.Exceptions;
using ShoppingListApi.Interfaces.Services;
using ShoppingListApi.Model.Database;
using ShoppingListApi.Model.DTOs.Get;
using ShoppingListApi.Model.DTOs.Patch;
using ShoppingListApi.Model.DTOs.PatchObsolete;
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
        /// user endpoint to get all the shopping lists for a user. The variable userId points to the list owner ID.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="requestingUserId"></param>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType<List<ShoppingListGetDto>>(StatusCodes.Status200OK)]
        [ProducesResponseType<string>(StatusCodes.Status500InternalServerError)]
        [Route("User/{userId:Guid}/ShoppingList/all")]
        public async Task<ActionResult> GetShoppingListsForUser([FromRoute] Guid userId,
            [FromHeader(Name = "USER-ID")] Guid? requestingUserId)
        {
            if (requestingUserId is null)
            {
                return Unauthorized(new AuthenticationErrorResponse(AuthorizationErrorEnum.UserCredentialsMissing));
            }

            if (!requestingUserId.Equals(userId))
            {
                return Unauthorized(new AuthenticationErrorResponse(AuthorizationErrorEnum.ListAccessNotGranted));
            }

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
        /// user endpoint for retrieving a single shopping list by id. The variable userId points to the list owner ID.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="shoppingListId"></param>
        /// <param name="requestingUserId"></param>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType<string>(StatusCodes.Status404NotFound)]
        [ProducesResponseType<AuthenticationErrorResponse>(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType<List<ShoppingListGetDto>>(StatusCodes.Status200OK)]
        [ProducesResponseType<string>(StatusCodes.Status500InternalServerError)]
        [Route("User/{userId:Guid}/ShoppingList/{shoppingListId:Guid}")]
        public async Task<ActionResult> GetShoppingListForUser([FromRoute] Guid userId, [FromRoute] Guid shoppingListId,
            [FromHeader(Name = "USER-ID")] Guid? requestingUserId)
        {
            if (requestingUserId is null)
            {
                return Unauthorized(new AuthenticationErrorResponse(AuthorizationErrorEnum.UserCredentialsMissing));
            }

            if (!requestingUserId.Equals(userId))
            {
                return Unauthorized(new AuthenticationErrorResponse(AuthorizationErrorEnum.ListAccessNotGranted));
            }

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
        /// user endpoint to add a shopping list for a user. The max allowed amount of shopping lists for every user as list owner is 5. The variable userId points to the list owner ID.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="shoppingListPostDto"></param>
        /// <param name="requestingUserId"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType<Guid>(StatusCodes.Status200OK)]
        [ProducesResponseType<string>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<string>(StatusCodes.Status500InternalServerError)]
        [Route("User/{userId:Guid}/ShoppingList")]
        public async Task<ActionResult> AddShoppingListForUser([FromRoute] Guid userId,
            [FromBody] ShoppingListPostDto shoppingListPostDto,
            [FromHeader(Name = "USER-ID")] Guid? requestingUserId)
        {
            if (requestingUserId is null)
            {
                return Unauthorized(new AuthenticationErrorResponse(AuthorizationErrorEnum.UserCredentialsMissing));
            }

            if (!requestingUserId.Equals(userId))
            {
                return Unauthorized(new AuthenticationErrorResponse(AuthorizationErrorEnum.ListAccessNotGranted));
            }

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
        /// user endpoint to add a new item to a shopping list. The maximum allowed amount of items for any shopping list is 20. The variable userId points to the list owner ID.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="shoppingListId"></param>
        /// <param name="itemPostDto"></param>
        /// <param name="requestingUserId"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType<Guid>(StatusCodes.Status200OK)]
        [ProducesResponseType<string>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<AuthenticationErrorResponse>(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType<string>(StatusCodes.Status500InternalServerError)]
        [Route("User/{userId:Guid}/ShoppingList/{shoppingListId:Guid}/Item")]
        public async Task<ActionResult> AddItemToShoppingList([FromRoute] Guid userId, [FromRoute] Guid shoppingListId,
            [FromBody] ItemPostDto itemPostDto, [FromHeader(Name = "USER-ID")] Guid? requestingUserId)
        {
            if (requestingUserId is null)
            {
                return Unauthorized(new AuthenticationErrorResponse(AuthorizationErrorEnum.UserCredentialsMissing));
            }

            if (!requestingUserId.Equals(userId))
            {
                return Unauthorized(new AuthenticationErrorResponse(AuthorizationErrorEnum.ListAccessNotGranted));
            }

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
        /// user endpoint to add collaborators to a shopping list by the list admin. The variable userId points to the list owner ID.
        /// </summary>
        /// <param name="requestingUserId"></param>
        /// <param name="userId"></param>
        /// <param name="shoppingListId"></param>
        /// <param name="collaboratorEmailAddress"></param>
        /// <returns></returns>
        /// 
        [HttpPost]
        [ProducesResponseType<Guid>(StatusCodes.Status200OK)]
        [ProducesResponseType<Guid>(StatusCodes.Status404NotFound)]
        [ProducesResponseType<AuthenticationErrorResponse>(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType<string>(StatusCodes.Status500InternalServerError)]
        [Route("User/{userId:Guid}/ShoppingList/{shoppingListId:Guid}/Collaborator/{collaboratorEmailAddress}")]
        public async Task<ActionResult> AddCollaboratorToShoppingList(
            [FromHeader(Name = "USER-ID")] Guid? requestingUserId,
            Guid userId, Guid shoppingListId, string collaboratorEmailAddress)
        {
            if (requestingUserId is null)
            {
                return Unauthorized(new AuthenticationErrorResponse(AuthorizationErrorEnum.UserCredentialsMissing));
            }

            if (requestingUserId != userId)
            {
                return Unauthorized(new AuthenticationErrorResponse(AuthorizationErrorEnum.ListAccessNotGranted));
            }

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
        /// user endpoint to modify the name of the shopping list by list owner. The variable userId points to the list owner ID.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="shoppingListId"></param>
        /// <param name="shoppingListPostDto"></param>
        /// <param name="requestingUserId"></param>
        /// <returns></returns>
        [HttpPatch]
        [ProducesResponseType<string>(StatusCodes.Status404NotFound)]
        [ProducesResponseType<AuthenticationErrorResponse>(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType<string>(StatusCodes.Status200OK)]
        [ProducesResponseType<string>(StatusCodes.Status500InternalServerError)]
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
        /// user endpoint to modify the details of an item in the shopping list. The variable userId points to the list owner ID.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="shoppingListId"></param>
        /// <param name="itemId"></param>
        /// <param name="itemPatchDto"></param>
        /// <param name="requestingUserId"></param>
        /// <returns></returns>
        [HttpPatch]
        [ProducesResponseType<string>(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType<string>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<string>(StatusCodes.Status404NotFound)]
        [ProducesResponseType<AuthenticationErrorResponse>(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType<string>(StatusCodes.Status200OK)]
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
        /// user endpoint to remove a shopping list for list owner. The variable userId points to the list owner ID.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="shoppingListId"></param>
        /// <param name="requestingUserId"></param>
        /// <returns></returns>
        [HttpDelete]
        [ProducesResponseType<string>(StatusCodes.Status200OK)]
        [ProducesResponseType<string>(StatusCodes.Status404NotFound)]
        [ProducesResponseType<AuthenticationErrorResponse>(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType<string>(StatusCodes.Status500InternalServerError)]
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
        /// user endpoint to remove an item from the shopping list. The variable userId points to the list owner ID.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="shoppingListId"></param>
        /// <param name="itemId"></param>
        /// <param name="requestingUserId"></param>
        /// <returns></returns>
        [HttpDelete]
        [ProducesResponseType<string>(StatusCodes.Status200OK)]
        [ProducesResponseType<AuthenticationErrorResponse>(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType<string>(StatusCodes.Status500InternalServerError)]
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
        /// user endpoint to remove a collaborator from a shopping list. Either the list owner kicks a collaborator or a collaborator leaves the list. The variable userId points to the list owner ID.
        /// </summary>
        /// <param name="requestingUserId"></param>
        /// <param name="userId"></param>
        /// <param name="shoppingListId"></param>
        /// <param name="collaboratorId"></param>
        /// <returns></returns>
        [HttpDelete]
        [ProducesResponseType<string>(StatusCodes.Status200OK)]
        [ProducesResponseType<string>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<AuthenticationErrorResponse>(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType<string>(StatusCodes.Status500InternalServerError)]
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
        /// user endpoint to remove a collaborator from a shopping list. Either the list owner kicks a collaborator or a collaborator leaves the list. The variable userId points to the list owner ID.
        /// </summary>
        /// <param name="requestingUserId"></param>
        /// <param name="userId"></param>
        /// <param name="shoppingListId"></param>
        /// <param name="collaboratorId"></param>
        /// <returns></returns>
        [HttpDelete]
        [ProducesResponseType<string>(StatusCodes.Status200OK)]
        [ProducesResponseType<string>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<AuthenticationErrorResponse>(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType<string>(StatusCodes.Status500InternalServerError)]
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
                $"Successfully removed! Amount of deleted records is attached."));
        }
    }
}