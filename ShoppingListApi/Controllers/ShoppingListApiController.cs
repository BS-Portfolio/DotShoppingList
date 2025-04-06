using System.ComponentModel.DataAnnotations;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.JavaScript;
using System.Text;
using Azure.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using ShoppingListApi.Attributes;
using ShoppingListApi.Configs;
using ShoppingListApi.Enums;
using ShoppingListApi.Exceptions;
using ShoppingListApi.Model.Database;
using ShoppingListApi.Model.Get;
using ShoppingListApi.Model.Patch;
using ShoppingListApi.Model.Post;
using ShoppingListApi.Model.ReturnTypes;
using ShoppingListApi.Services;
using Xunit.Sdk;

namespace ShoppingListApi.Controllers;

[ApiController]
[Route("[controller]")]
public class ShoppingListApiController : ControllerBase
{
    private readonly DatabaseService _databaseService;
    private readonly ILogger<ShoppingListApiController> _logger;

    public ShoppingListApiController(IServiceProvider serviceProvider)
    {
        _databaseService = serviceProvider.GetRequiredService<DatabaseService>();
        _logger = serviceProvider.GetRequiredService<ILogger<ShoppingListApiController>>();
    }

    [HttpGet]
    [PublicEndpoint]
    [ProducesResponseType<string>(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<List<UserRole>>(StatusCodes.Status200OK)]
    [Route("UserRole/all")]
    public async Task<ActionResult> GetUserRoles()
    {
        try
        {
            List<UserRole> userRoles = await _databaseService.SqlConnectionHandlerAsync<List<UserRole>>(
                async (connection) => await _databaseService.GetUserRolesAsync(connection)
            );

            if (userRoles.Count == 0)
            {
                return NoContent();
            }

            return Ok(userRoles);
        }
        catch (NumberedException nEx)
        {
            _logger.LogWithLevel(LogLevel.Error, nEx, nEx.ErrorNumber, nEx.Message,
                nameof(ShoppingListApiController), nameof(GetUserRoles));
            return StatusCode(StatusCodes.Status500InternalServerError,
                "Du to an internal error, your request could not be processed.");
        }
        catch (Exception e)
        {
            var numberedException = new NumberedException(e);
            _logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
                nameof(ShoppingListApiController), nameof(GetUserRoles));
            return StatusCode(StatusCodes.Status500InternalServerError,
                "Du to an internal error, your request could not be processed.");
        }
    }

    [HttpGet]
    [AdminEndpoint]
    [ProducesResponseType<string>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ListUser>(StatusCodes.Status200OK)]
    [ProducesResponseType<string>(StatusCodes.Status500InternalServerError)]
    [Route("User/EmailAddress/{emailAddress}")]
    public async Task<ActionResult> GetUserByEmail([FromRoute] string emailAddress)
    {
        try
        {
            var user = await _databaseService.SqlConnectionHandlerAsync<string, ListUser?>(
                (input, connection) => _databaseService.GetUserByEmailAddressAsync(input, connection),
                emailAddress
            );

            if (user is null)
            {
                return NotFound($"User with email {emailAddress} not found");
            }

            return Ok(user);
        }
        catch (NumberedException nEx)
        {
            _logger.LogWithLevel(LogLevel.Error, nEx, nEx.ErrorNumber, nEx.Message,
                nameof(ShoppingListApiController), nameof(GetUserByEmail));
            return StatusCode(StatusCodes.Status500InternalServerError,
                "Due to an internal error, your request could not be processed.");
        }
        catch (Exception e)
        {
            var numberedException = new NumberedException(e);
            _logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
                nameof(ShoppingListApiController), nameof(GetUserByEmail));
            return StatusCode(StatusCodes.Status500InternalServerError,
                "Due to an internal error, your request could not be processed.");
        }
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<List<ShoppingList>>(StatusCodes.Status200OK)]
    [ProducesResponseType<string>(StatusCodes.Status500InternalServerError)]
    [Route("User/{userId:Guid}/ShoppingList/all")]
    public async Task<ActionResult> GetShoppingListsForUser([FromRoute] Guid userId)
    {
        try
        {
            var shoppingLists = await _databaseService.SqlConnectionHandlerAsync<Guid, List<ShoppingList>>(
                async (id, connection) => await _databaseService.HandleShoppingListsFetchForUserAsync(id, connection),
                userId
            );

            if (shoppingLists.Count == 0)
            {
                return NoContent();
            }

            return Ok(shoppingLists);
        }
        catch (NumberedException nEx)
        {
            _logger.LogWithLevel(LogLevel.Error, nEx, nEx.ErrorNumber, nEx.Message,
                nameof(ShoppingListApiController), nameof(GetShoppingListsForUser));
            return StatusCode(StatusCodes.Status500InternalServerError,
                "Due to an internal error, your request could not be processed.");
        }
        catch (Exception e)
        {
            var numberedException = new NumberedException(e);
            _logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
                nameof(ShoppingListApiController), nameof(GetShoppingListsForUser));
            return StatusCode(StatusCodes.Status500InternalServerError,
                "Due to an internal error, your request could not be processed.");
        }
    }

    [HttpGet]
    [AdminEndpoint]
    [ProducesResponseType<string>(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType<ListUser>(StatusCodes.Status200OK)]
    [ProducesResponseType<string>(StatusCodes.Status404NotFound)]
    [Route("User/{userId:Guid}")]
    public async Task<ActionResult> GetUserById(Guid userId)
    {
        try
        {
            var user = await _databaseService.SqlConnectionHandlerAsync<Guid, ListUser?>(
                (input, connection) => _databaseService.GetUserByIdAsync(input, connection),
                userId
            );

            if (user is null)
            {
                return NotFound($"User with the provided ID not found");
            }

            return Ok(user);
        }
        catch (NumberedException nEx)
        {
            _logger.LogWithLevel(LogLevel.Error, nEx, nEx.ErrorNumber, nEx.Message,
                nameof(ShoppingListApiController), nameof(GetUserById));
            return StatusCode(StatusCodes.Status500InternalServerError,
                "Due to an internal error, your request could not be processed.");
        }
        catch (Exception e)
        {
            var numberedException = new NumberedException(e);
            _logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
                nameof(ShoppingListApiController), nameof(GetUserById));
            return StatusCode(StatusCodes.Status500InternalServerError,
                "Due to an internal error, your request could not be processed.");
        }
    }

    [HttpGet]
    [AdminEndpoint]
    [ProducesResponseType<List<ListUserMinimal>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<string>(StatusCodes.Status500InternalServerError)]
    [Route("User/all")]
    public async Task<ActionResult> GetAllUsers()
    {
        try
        {
            var users = await _databaseService.SqlConnectionHandlerAsync<List<ListUserMinimal>>(
                async connection => await _databaseService.GetAllUsersAsync(connection));

            if (users.Count == 0)
            {
                return NoContent();
            }

            return Ok(users);
        }
        catch (NumberedException nEx)
        {
            _logger.LogWithLevel(LogLevel.Error, nEx, nEx.ErrorNumber, nEx.Message,
                nameof(ShoppingListApiController), nameof(GetAllUsers));
            return StatusCode(StatusCodes.Status500InternalServerError,
                "Due to an internal error, your request could not be processed.");
        }
        catch (Exception e)
        {
            var numberedException = new NumberedException(e);
            _logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
                nameof(ShoppingListApiController), nameof(GetAllUsers));
            return StatusCode(StatusCodes.Status500InternalServerError,
                "Due to an internal error, your request could not be processed.");
        }
    }

    [HttpPost]
    [AdminEndpoint]
    [ProducesResponseType<string>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<Guid>(StatusCodes.Status201Created)]
    [ProducesResponseType<string>(StatusCodes.Status500InternalServerError)]
    [Route("UserRole")]
    public async Task<ActionResult> AddUserRole([FromBody] UserRolePost userRolePost)
    {
        try
        {
            var alreadyExists = await _databaseService.SqlConnectionHandlerAsync<int, bool>(
                async (enumIndex, sqlConnection) =>
                    await _databaseService.CheckUserRoleExistenceAsync(enumIndex, sqlConnection),
                (int)userRolePost.UserRoleEnum
            );

            if (alreadyExists)
            {
                return Conflict("A user role withe same Enum index already exists in the database.");
            }

            var (success, addedUserRoleId) =
                await _databaseService.SqlConnectionHandlerAsync<UserRolePost, (bool, Guid?)>(
                    (input, connection) => _databaseService.AddUserRoleAsync(connection, input), userRolePost);

            if (success is false)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Du to an internal error, your request could not be processed.");
            }

            return CreatedAtAction(nameof(AddUserRole), addedUserRoleId);
        }
        catch (NumberedException nEx)
        {
            _logger.LogWithLevel(LogLevel.Error, nEx, nEx.ErrorNumber, nEx.Message,
                nameof(ShoppingListApiController), nameof(AddUserRole));
            return StatusCode(StatusCodes.Status500InternalServerError,
                "Du to an internal error, your request could not be processed.");
        }
        catch (Exception e)
        {
            var numberedException = new NumberedException(e);
            _logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
                nameof(ShoppingListApiController), nameof(AddUserRole));
            return StatusCode(StatusCodes.Status500InternalServerError,
                "Du to an internal error, your request could not be processed.");
        }
    }

    [HttpPost]
    [PublicEndpoint]
    [ProducesResponseType<string>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<Guid>(StatusCodes.Status201Created)]
    [ProducesResponseType<string>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<string>(StatusCodes.Status500InternalServerError)]
    [Route("User")]
    public async Task<ActionResult> AddNewUser([FromBody] ListUserPost userPost)
    {
        try
        {
            var emailAlreadyExists = await _databaseService.SqlConnectionHandlerAsync<string, bool>(
                async (emailAddress, sqlConnection) =>
                    await _databaseService.CheckUserExistenceAsync(emailAddress, sqlConnection), userPost.EmailAddress);

            if (emailAlreadyExists)
            {
                return Conflict(
                    "A user with the same email address already exists in the database. Use a different address to register yourself!");
            }

            var userPostExtended = new ListUserPostExtended(userPost);

            var (success, addedUserId) =
                await _databaseService.SqlConnectionHandlerAsync<ListUserPostExtended, (bool, Guid?)>(
                    (input, connection) => _databaseService.AddUserAsync(input, connection), userPostExtended);

            if (success is false || addedUserId is null)
            {
                return Problem("Due to an internal error, you request could not be processed.");
            }

            return Ok(addedUserId);
        }
        catch (FormatException fEx)
        {
            _logger.LogWithLevel(LogLevel.Error, fEx, "0", fEx.Message,
                nameof(ShoppingListApiController), nameof(AddUserRole));
            return BadRequest("Your input email address and password were delivered in wrong formatting!");
        }
        catch (NumberedException nEx)
        {
            _logger.LogWithLevel(LogLevel.Error, nEx, nEx.ErrorNumber, nEx.Message,
                nameof(ShoppingListApiController), nameof(AddUserRole));
            return StatusCode(StatusCodes.Status500InternalServerError,
                "Due to an internal error, your request could not be processed.");
        }
        catch (Exception e)
        {
            var numberedException = new NumberedException(e);
            _logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
                nameof(ShoppingListApiController), nameof(AddNewUser));
            return StatusCode(StatusCodes.Status500InternalServerError,
                "Due to an internal error, your request could not be processed.");
        }
    }

    [HttpPost]
    [Route("User/Login")]
    [PublicEndpoint]
    [ProducesResponseType<ListUser>(StatusCodes.Status200OK)]
    [ProducesResponseType<string>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<string>(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> UserLogin([FromHeader] string emailAddress, [FromHeader] string password)
    {
        try
        {
            var user = await _databaseService.SqlConnectionHandlerAsync<LoginData, ListUser?>(
                async (loginData, sqlConnection) => await _databaseService.HandleLoginAsync(loginData, sqlConnection),
                new LoginData(emailAddress, password));

            if (user is null)
            {
                return Unauthorized("Your credentials are not valid!");
            }

            return Ok(user);
        }
        catch (NoContentFoundException<string> ncEx)
        {
            _logger.LogWithLevel(LogLevel.Error, ncEx, ncEx.ErrorNumber, ncEx.Message,
                nameof(ShoppingListApiController), nameof(UserLogin));
            return NotFound("Not user account found for the provided email address.");
        }
        catch (MultipleUsersForEmailException mEx)
        {
            _logger.LogWithLevel(LogLevel.Error, mEx, mEx.ErrorNumber, mEx.Message,
                nameof(ShoppingListApiController), nameof(UserLogin));
            return StatusCode(StatusCodes.Status500InternalServerError,
                "Due to an internal error, your request could not be processed. Your email address has been registered under multiple users!");
        }
        catch (NumberedException nEx)
        {
            _logger.LogWithLevel(LogLevel.Error, nEx, nEx.ErrorNumber, nEx.Message,
                nameof(ShoppingListApiController), nameof(UserLogin));
            return StatusCode(StatusCodes.Status500InternalServerError,
                "Due to an internal error, your request could not be processed.");
        }
        catch (Exception e)
        {
            var numberedException = new NumberedException(e);
            _logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
                nameof(ShoppingListApiController), nameof(UserLogin));
            return StatusCode(StatusCodes.Status500InternalServerError,
                "Due to an internal error, your request could not be processed.");
        }
    }

    [HttpPost]
    [ProducesResponseType<Guid>(StatusCodes.Status200OK)]
    [ProducesResponseType<string>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<string>(StatusCodes.Status500InternalServerError)]
    [Route("User/{userId:Guid}/ShoppingList")]
    public async Task<ActionResult> AddShoppingListForUser([FromRoute] Guid userId, [FromBody] string shoppingListName)
    {
        try
        {
            var result = await _databaseService
                .SqlConnectionHandlerAsync<ShoppingListAdditionData, ShoppingListAdditionResult>(
                    async (data, sqlConnection) =>
                        await _databaseService.HandleAddingShoppingListAsync(data, sqlConnection),
                    new ShoppingListAdditionData(shoppingListName, userId));

            if (result.Success is false || result.AddedShoppingListId is null)
            {
                if (result.NameAlreadyExists)
                {
                    return Conflict($"You already have a shopping list named: \"{shoppingListName}\"");
                }

                if (result.MaximumNumberOfListsReached)
                {
                    return BadRequest("Maximum number of shopping lists reached!");
                }

                if (result.AddedShoppingListId is null)
                {
                    return Problem("Due to an internal error your request could not be processed.");
                }

                if (result.ListAssignmentSuccess is false)
                {
                    if (result.AddedShoppingListId is not null)
                    {
                        await _databaseService.SqlConnectionHandlerAsync<Guid, bool>(
                            async (input, connection) =>
                                await _databaseService.RemoveShoppingListByIdAsync(input, connection),
                            (Guid)result.AddedShoppingListId);
                    }

                    return Problem("Due to an internal error your request could not be processed.");
                }
            }

            return Ok(result.AddedShoppingListId);
        }
        catch (NumberedException nEx)
        {
            _logger.LogWithLevel(LogLevel.Error, nEx, nEx.ErrorNumber, nEx.Message,
                nameof(ShoppingListApiController), nameof(AddShoppingListForUser));
            return StatusCode(StatusCodes.Status500InternalServerError,
                "Due to an internal error, your request could not be processed.");
        }
        catch (Exception e)
        {
            var numberedException = new NumberedException(e);
            _logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
                nameof(ShoppingListApiController), nameof(AddShoppingListForUser));
            return StatusCode(StatusCodes.Status500InternalServerError,
                "Due to an internal error, your request could not be processed.");
        }
    }

    [HttpPost]
    [ProducesResponseType<Guid>(StatusCodes.Status200OK)]
    [ProducesResponseType<string>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<AuthenticationErrorResponse>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<string>(StatusCodes.Status500InternalServerError)]
    [Route("User/{userId:Guid}/ShoppingList/{shoppingListId:Guid}/Item")]
    public async Task<ActionResult> AddItemToShoppingList([FromRoute] Guid userId, [FromRoute] Guid shoppingListId,
        [FromBody] ItemPost itemPost)
    {
        try
        {
            var result = await _databaseService.SqlConnectionHandlerAsync<NewItemData, ItemAdditionResult>(
                async (data, connection) =>
                    await _databaseService.HandleAddingItemToShoppingListAsync(data, connection),
                new NewItemData(itemPost, shoppingListId, userId));

            if (result.Success is false || result.ItemId is null)
            {
                if (result.AccessGranted is false)
                {
                    return Unauthorized(new AuthenticationErrorResponse(AuthorizationErrorEnum.ListAccessNotGranted));
                }

                if (result.MaximumCountReached)
                {
                    return BadRequest("You have reached the maximum number of items in a list.");
                }

                return Problem("Due to an internal error your request could not be processed.");
            }

            return Ok(result.ItemId);
        }
        catch (NumberedException nEx)
        {
            _logger.LogWithLevel(LogLevel.Error, nEx, nEx.ErrorNumber, nEx.Message,
                nameof(ShoppingListApiController), nameof(AddItemToShoppingList));
            return StatusCode(StatusCodes.Status500InternalServerError,
                "Due to an internal error, your request could not be processed.");
        }
        catch (Exception e)
        {
            var numberedException = new NumberedException(e);
            _logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
                nameof(ShoppingListApiController), nameof(AddItemToShoppingList));
            return StatusCode(StatusCodes.Status500InternalServerError,
                "Due to an internal error, your request could not be processed.");
        }
    }

    [HttpPost]
    [Route("User/{userId:Guid}/ShoppingList/{shoppingListId:Guid}/Collaborator/{collaboratorEmailAddress}")]
    public async Task<ActionResult> AddCollaboratorToShoppingList([FromHeader(Name = "USER-ID")] Guid? requestingUserId,
        Guid userId, Guid shoppingListId, string collaboratorEmailAddress)
    {
        try
        {
            if (requestingUserId is null)
            {
                return Unauthorized(new AuthenticationErrorResponse(AuthorizationErrorEnum.UserCredentialsMissing));
            }

            var result = await _databaseService
                .SqlConnectionHandlerAsync<CollaboratorAdditionData, CollaboratorAddRemoveResult>(
                    async (input, connection) =>
                        await _databaseService.HandleAddingCollaboratorToShoppingListAsync(input, connection),
                    new CollaboratorAdditionData(userId, shoppingListId, collaboratorEmailAddress,
                        (Guid)requestingUserId));

            if (result.Success is false)
            {
                if (result.ListOwnerIdIsValid is false)
                {
                    return Unauthorized(new AuthenticationErrorResponse(AuthorizationErrorEnum.ListAccessNotGranted));
                }

                if (result.CollaboratorIdIsValid is false)
                {
                    return NotFound("A user with the provided email address was not found.");
                }

                if (result.RequestingPartyIdIsValid is false)
                {
                    return Unauthorized(new AuthenticationErrorResponse(AuthorizationErrorEnum.ActionNotAllowed));
                }

                return Problem("Due to an internal error, your request could not be processed.");
            }

            return Ok("Collaborator successfully added to the list!");
        }
        catch (NumberedException nEx)
        {
            _logger.LogWithLevel(LogLevel.Error, nEx, nEx.ErrorNumber, nEx.Message,
                nameof(ShoppingListApiController), nameof(AddCollaboratorToShoppingList));
            return StatusCode(StatusCodes.Status500InternalServerError,
                "Due to an internal error, your request could not be processed.");
        }
        catch (Exception e)
        {
            var numberedException = new NumberedException(e);
            _logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
                nameof(ShoppingListApiController), nameof(AddCollaboratorToShoppingList));
            return StatusCode(StatusCodes.Status500InternalServerError,
                "Due to an internal error, your request could not be processed.");
        }
    }


    [HttpPatch]
    [ProducesResponseType<string>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<string>(StatusCodes.Status200OK)]
    [ProducesResponseType<string>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<string>(StatusCodes.Status500InternalServerError)]
    [Route("User/{userId:Guid}")]
    public async Task<ActionResult> ModifyUserDetails([FromRoute] Guid userId, [FromBody] ListUserPatch listUserPatch)
    {
        if (listUserPatch.NewFirstName is null &&
            listUserPatch.NewLastName is null)
        {
            return BadRequest("Nothing to update!");
        }

        try
        {
            var success = await _databaseService.SqlConnectionHandlerAsync<ModificationData<Guid, ListUserPatch>, bool>(
                async (input, connection) => await _databaseService.ModifyUserDetailsAsync(input, connection),
                new ModificationData<Guid, ListUserPatch>(userId, listUserPatch));

            if (success is false) return NotFound($"A user with the provided ID {userId} was not found.");

            return Ok("Update successful!");
        }
        catch (NumberedException nEx)
        {
            _logger.LogWithLevel(LogLevel.Error, nEx, nEx.ErrorNumber, nEx.Message,
                nameof(ShoppingListApiController), nameof(ModifyUserDetails));
            return StatusCode(StatusCodes.Status500InternalServerError,
                "Due to an internal error, your request could not be processed.");
        }
        catch (Exception e)
        {
            var numberedException = new NumberedException(e);
            _logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
                nameof(ShoppingListApiController), nameof(ModifyUserDetails));
            return StatusCode(StatusCodes.Status500InternalServerError,
                "Due to an internal error, your request could not be processed.");
        }
    }

    [HttpPatch]
    [ProducesResponseType<string>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<AuthenticationErrorResponse>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<string>(StatusCodes.Status200OK)]
    [ProducesResponseType<string>(StatusCodes.Status500InternalServerError)]
    [Route("User/{userId:Guid}/ShoppingList/{shoppingListId}")]
    public async Task<ActionResult> ModifyShoppingListName([FromRoute] Guid userId, [FromRoute] Guid shoppingListId,
        [FromBody] ShoppingListPatch shoppingListPatch)
    {
        try
        {
            var result = await _databaseService
                .SqlConnectionHandlerAsync<ModificationData<(Guid userId, Guid shoppingListId), ShoppingListPatch>,
                    UpdateResult>(
                    (input, connection) => _databaseService.HandleShoppingListNameUpdateAsync(input, connection)
                    , new ModificationData<(Guid userId, Guid shoppingListId), ShoppingListPatch>((userId, shoppingListId), shoppingListPatch));

            if (result.Success is false)
            {
                if (result.AccessGranted)
                {
                    return NotFound("No shopping list found for the provided ID's");
                }

                return Unauthorized(new AuthenticationErrorResponse(AuthorizationErrorEnum.ListAccessNotGranted));
            }

            return Ok("Update Successful!");
        }
        catch (NumberedException nEx)
        {
            _logger.LogWithLevel(LogLevel.Error, nEx, nEx.ErrorNumber, nEx.Message,
                nameof(ShoppingListApiController), nameof(ModifyShoppingListName));
            return StatusCode(StatusCodes.Status500InternalServerError,
                "Due to an internal error, your request could not be processed.");
        }
        catch (Exception e)
        {
            var numberedException = new NumberedException(e);
            _logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
                nameof(ShoppingListApiController), nameof(ModifyShoppingListName));
            return StatusCode(StatusCodes.Status500InternalServerError,
                "Due to an internal error, your request could not be processed.");
        }
    }

    [HttpPatch]
    [ProducesResponseType<string>(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType<string>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<string>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<AuthenticationErrorResponse>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<string>(StatusCodes.Status200OK)]
    [Route("User/{userId:Guid}/ShoppingList/{shoppingListId}/Item/{itemId:Guid}")]
    public async Task<ActionResult> ModifyItemDetails(Guid userId, Guid shoppingListId, Guid itemId,
        ItemPatch itemPatch)
    {
        if (itemPatch.NewItemAmount is null &&
            itemPatch.NewItemName is null)
        {
            return BadRequest("Nothing to update!");
        }

        try
        {
            var result = await _databaseService
                .SqlConnectionHandlerAsync<ModificationData<(Guid userId, Guid shoppingListId, Guid itemId), ItemPatch>,
                    UpdateResult>(
                    (input, connection) => _databaseService.HandleShoppingListItemUpdateAsync(input, connection)
                    , new ModificationData<(Guid userId, Guid shoppingListId, Guid itemId), ItemPatch>((userId, shoppingListId, itemId), itemPatch));

            if (result.Success is false)
            {
                if (result.AccessGranted)
                {
                    return NotFound("No shopping list item found for the provided ID's");
                }

                return Unauthorized(new AuthenticationErrorResponse(AuthorizationErrorEnum.ListAccessNotGranted));
            }

            return Ok("Update Successful!");
        }
        catch (NumberedException nEx)
        {
            _logger.LogWithLevel(LogLevel.Error, nEx, nEx.ErrorNumber, nEx.Message,
                nameof(ShoppingListApiController), nameof(ModifyShoppingListName));
            return StatusCode(StatusCodes.Status500InternalServerError,
                "Due to an internal error, your request could not be processed.");
        }
        catch (Exception e)
        {
            var numberedException = new NumberedException(e);
            _logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
                nameof(ShoppingListApiController), nameof(ModifyShoppingListName));
            return StatusCode(StatusCodes.Status500InternalServerError,
                "Due to an internal error, your request could not be processed.");
        }
    }

    [HttpDelete]
    [AdminEndpoint]
    [ProducesResponseType<string>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<int>(StatusCodes.Status200OK)]
    [ProducesResponseType<string>(StatusCodes.Status500InternalServerError)]
    [Route("User/{emailAddress}")]
    public async Task<ActionResult> RemoveUserByEmailAddress([FromRoute] string emailAddress)
    {
        try
        {
            var result = await _databaseService
                .SqlConnectionHandlerAsync<string, UserRemovalDbResult>(
                    async (input, connection) => await _databaseService.RemoveUserByEmailAsync(input, connection)
                    , emailAddress);

            if (result.Success is false)
            {
                if (result.UserExists is false)
                    return NotFound("A user with the provided email address does not exists!");

                return Problem("Due to an internal error, your request could not be processed! ");
            }

            return Ok(result.RemovedShoppingListsCount);
        }
        catch (NumberedException nEx)
        {
            _logger.LogWithLevel(LogLevel.Error, nEx, nEx.ErrorNumber, nEx.Message,
                nameof(ShoppingListApiController), nameof(RemoveUserByEmailAddress));
            return StatusCode(StatusCodes.Status500InternalServerError,
                "Due to an internal error, your request could not be processed.");
        }
        catch (Exception e)
        {
            var numberedException = new NumberedException(e);
            _logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
                nameof(ShoppingListApiController), nameof(RemoveUserByEmailAddress));
            return StatusCode(StatusCodes.Status500InternalServerError,
                "Due to an internal error, your request could not be processed.");
        }
    }

    [HttpDelete]
    [Route("User/{userId:Guid}/ShoppingList/{shoppingListId:Guid}")]
    public async Task<ActionResult> RemoveShoppingList([FromRoute] Guid userId, [FromRoute] Guid shoppingListId)
    {
        try
        {
            var result = await _databaseService
                .SqlConnectionHandlerAsync<ShoppingListIdentificationData, ShoppingListRemovalResult>(
                    async (input, connection) =>
                        await _databaseService.HandleShoppingListRemovalAsync(input, connection)
                    , new ShoppingListIdentificationData(userId, shoppingListId));

            if (result.Success is false)
            {
                if (result.Exists is false)
                {
                    return NotFound("A shopping list with the provided ID does not exists");
                }

                if (result.AccessGranted is false)
                {
                    return Unauthorized(new AuthenticationErrorResponse(AuthorizationErrorEnum.ListAccessNotGranted));
                }
            }

            return Ok("Successfully removed!");
        }
        catch (NumberedException nEx)
        {
            _logger.LogWithLevel(LogLevel.Error, nEx, nEx.ErrorNumber, nEx.Message,
                nameof(ShoppingListApiController), nameof(RemoveShoppingList));
            return StatusCode(StatusCodes.Status500InternalServerError,
                "Due to an internal error, your request could not be processed.");
        }
        catch (Exception e)
        {
            var numberedException = new NumberedException(e);
            _logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
                nameof(ShoppingListApiController), nameof(RemoveShoppingList));
            return StatusCode(StatusCodes.Status500InternalServerError,
                "Due to an internal error, your request could not be processed.");
        }
    }

    [HttpDelete]
    [Route(("User/{userId:Guid}/ShoppingList/{shoppingListId:Guid}/Item/{itemId:Guid}"))]
    public async Task<ActionResult> RemoveItemFromShoppingList([FromRoute] Guid userId, [FromRoute] Guid shoppingListId,
        [FromRoute] Guid itemId)
    {
        try
        {
            var result = await _databaseService.SqlConnectionHandlerAsync<ItemIdentificationData, UpdateResult>(
                async (input, connection) =>
                    await _databaseService.HandleItemRemovalFromShoppingListAsync(input, connection)
                , new ItemIdentificationData(userId, shoppingListId, itemId));

            if (result.Success is false)
            {
                if (result.AccessGranted)
                    return Problem(
                        "Due to an internal error, you request could not be processed. Most probable a shopping list with the provided ID does nto exists!");

                return Unauthorized(new AuthenticationErrorResponse(AuthorizationErrorEnum.ListAccessNotGranted));
            }

            return Ok("Successfully removed!");
        }
        catch (NumberedException nEx)
        {
            _logger.LogWithLevel(LogLevel.Error, nEx, nEx.ErrorNumber, nEx.Message,
                nameof(ShoppingListApiController), nameof(RemoveItemFromShoppingList));
            return StatusCode(StatusCodes.Status500InternalServerError,
                "Due to an internal error, your request could not be processed.");
        }
        catch (Exception e)
        {
            var numberedException = new NumberedException(e);
            _logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
                nameof(ShoppingListApiController), nameof(RemoveItemFromShoppingList));
            return StatusCode(StatusCodes.Status500InternalServerError,
                "Due to an internal error, your request could not be processed.");
        }
    }

    [HttpDelete]
    [Route("User/{userId:Guid}/ShoppingList/{shoppingListId:Guid}/Collaborator/{collaboratorId:Guid}")]
    public async Task<ActionResult> RemoveCollaboratorFromList([FromHeader(Name = "USER-ID")] Guid? requestingUserId,
        [FromRoute] Guid userId, [FromRoute] Guid shoppingListId, [FromRoute] Guid collaboratorId)
    {
        try
        {
            if (requestingUserId is null)
            {
                return Unauthorized(new AuthenticationErrorResponse(AuthorizationErrorEnum.UserCredentialsMissing));
            }

            var result = await
                _databaseService.SqlConnectionHandlerAsync<CollaboratorRemovalCheck, CollaboratorAddRemoveResult>(
                    async (input, connection) =>
                        await _databaseService.HandleCollaboratorRemovalAsync(input, connection),
                    new CollaboratorRemovalCheck(userId, shoppingListId, collaboratorId, (Guid)requestingUserId));

            if (result.Success is false)
            {
                if (result.ListOwnerIdIsValid is false)
                {
                    return BadRequest("The list owner id in not valid!");
                }

                if (result.CollaboratorIdIsValid is false)
                {
                    return BadRequest("The collaborator id is not valid!");
                }

                if (result.RequestingPartyIdIsValid is false)
                {
                    return Unauthorized(new AuthenticationErrorResponse(AuthorizationErrorEnum.ActionNotAllowed));
                }

                return Problem("Due to an internal error, your request could not be processed!");
            }

            return Ok("Successfully removed!");
        }
        catch (NumberedException nEx)
        {
            _logger.LogWithLevel(LogLevel.Error, nEx, nEx.ErrorNumber, nEx.Message,
                nameof(ShoppingListApiController), nameof(RemoveCollaboratorFromList));
            return StatusCode(StatusCodes.Status500InternalServerError,
                "Due to an internal error, your request could not be processed.");
        }
        catch (Exception e)
        {
            var numberedException = new NumberedException(e);
            _logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
                nameof(ShoppingListApiController), nameof(RemoveCollaboratorFromList));
            return StatusCode(StatusCodes.Status500InternalServerError,
                "Due to an internal error, your request could not be processed.");
        }
    }
}