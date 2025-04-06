using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;
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
            List<UserRole> userRoles = await _databaseService.SqlConnectionHandler<List<UserRole>>(
                async (connection) => await _databaseService.GetUserRoles(connection)
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
            var user = await _databaseService.SqlConnectionHandler<string, ListUser?>(
                (input, connection) => _databaseService.GetUserByEmailAddress(input, connection),
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
            var shoppingLists = await _databaseService.SqlConnectionHandler<Guid, List<ShoppingList>>(
                async (id, connection) => await _databaseService.HandleShoppingListsFetchForUser(id, connection),
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
            var user = await _databaseService.SqlConnectionHandler<Guid, ListUser?>(
                (input, connection) => _databaseService.GetUserById(input, connection),
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
            var users = await _databaseService.SqlConnectionHandler<List<ListUserMinimal>>(
                async connection => await _databaseService.GetAllUsers(connection));

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
            var alreadyExists = await _databaseService.SqlConnectionHandler<int, bool>(
                async (enumIndex, sqlConnection) =>
                    await _databaseService.CheckUserRoleExistence(enumIndex, sqlConnection),
                (int)userRolePost.UserRoleEnum
            );

            if (alreadyExists)
            {
                return Conflict("A user role withe same Enum index already exists in the database.");
            }

            var (success, addedUserRoleId) = await _databaseService.SqlConnectionHandler<UserRolePost, (bool, Guid?)>(
                (input, connection) => _databaseService.AddUserRole(connection, input), userRolePost);

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
            var emailAlreadyExists = await _databaseService.SqlConnectionHandler<string, bool>(
                async (emailAddress, sqlConnection) =>
                    await _databaseService.CheckUserExistence(emailAddress, sqlConnection), userPost.EmailAddress);

            if (emailAlreadyExists)
            {
                return Conflict(
                    "A user with the same email address already exists in the database. Use a different address to register yourself!");
            }

            var userPostExtended = new ListUserPostExtended(userPost);

            var (success, addedUserId) =
                await _databaseService.SqlConnectionHandler<ListUserPostExtended, (bool, Guid?)>(
                    (input, connection) => _databaseService.AddUser(input, connection), userPostExtended);

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
            var user = await _databaseService.SqlConnectionHandler<LoginData, ListUser?>(
                async (loginData, sqlConnection) => await _databaseService.HandleLogin(loginData, sqlConnection),
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
                .SqlConnectionHandler<ShoppingListAdditionData, ShoppingListAdditionResult>(
                    async (data, sqlConnection) => await _databaseService.HandleAddingShoppingList(data, sqlConnection),
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
                        await _databaseService.SqlConnectionHandler<Guid, bool>(
                            async (input, connection) =>
                                await _databaseService.RemoveShoppingListById(input, connection),
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
    [ProducesResponseType<string>(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType<string>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<AuthenticationErrorResponse>(StatusCodes.Status401Unauthorized)]
    [Route("User/{userId:Guid}/ShoppingList/{shoppingListId:Guid}/Item")]
    public async Task<ActionResult> AddItemToShoppingList([FromRoute] Guid userId, [FromRoute] Guid shoppingListId,
        [FromBody] ItemPost itemPost)
    {
        try
        {
            var result = await _databaseService.SqlConnectionHandler<NewItemData, ItemAdditionResult>(
                async (data, connection) => await _databaseService.HandleAddingItemToShoppingList(data, connection),
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

    [HttpPatch]
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
            var success = await _databaseService.SqlConnectionHandler<ModificationData<Guid, ListUserPatch>, bool>(
                async (input, connection) => await _databaseService.ModifyUserDetails(input, connection),
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
    [Route("User/{userId:Guid}/ShoppingList/{shoppingListId}")]
    public async Task<ActionResult> ModifyShoppingListName([FromRoute] Guid userId, [FromRoute] Guid shoppingListId,
        [FromBody] ShoppingListPatch shoppingListPatch)
    {
        try
        {
            var result = await _databaseService
                .SqlConnectionHandler<ModificationData<(Guid userId, Guid shoppingListId), ShoppingListPatch>,
                    UpdateResult>(
                    (input, connection) => _databaseService.HandleShoppingListNameUpdate(input, connection)
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
    [Route("User/{userId:Guid}/ShoppingList/{shoppingListId}/Item/{itemId:Guid}")]
    public async Task<ActionResult> ModifyItemDetails(Guid userId, Guid shoppingListId, Guid itemId, ItemPatch itemPatch)
    {
        if (itemPatch.NewItemAmount is null &&
            itemPatch.NewItemName is null)
        {
            return BadRequest("Nothing to update!");
        }
        
        try
        {
            var result = await _databaseService
                .SqlConnectionHandler<ModificationData<(Guid userId, Guid shoppingListId, Guid itemId), ItemPatch>,
                    UpdateResult>(
                    (input, connection) => _databaseService.HandleShoppingListItemUpdate(input, connection)
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
    /*


    [HttpDelete]
public async Task<ActionResult>  RemoveUser(){
}

     [HttpDelete]
public async Task<ActionResult>  RemoveShoppingList(){
}

    [HttpDelete]
public async Task<ActionResult>  RemoveItemFromShoppingList(){
}

    [HttpDelete]
public async Task<ActionResult>  RemoveCollaboratorFromList(){
}

    [HttpDelete]
public async Task<ActionResult>  LeaveListAsCollaborator(){
}



    // PATCH-Methoden
    [Route("ShoppingList/{listId:guid}")]
    public async Task<ActionResult> UpdateShoppingList(Guid listId, [FromBody] Model.Patch.ShoppingListPatch listPatch)
    {
        try
        {
            bool success = await _databaseService.SqlConnectionHandler<(Guid, Model.Patch.ShoppingListPatch), bool>(
                (input, connection) => _databaseService.UpdateShoppingList(input.Item1, input.Item2, connection),
                (listId, listPatch)
            );

            if (!success)
            {
                return NotFound($"Einkaufsliste mit ID {listId} nicht gefunden");
            }

            return NoContent();
        }
        catch (NumberedException nEx)
        {
            _logger.LogWithLevel(LogLevel.Error, nEx, nEx.ErrorNumber, nEx.Message,
                nameof(ShoppingListController), nameof(UpdateShoppingList));
            return StatusCode(StatusCodes.Status500InternalServerError,
                "Aufgrund eines internen Fehlers konnte deine Anfrage nicht verarbeitet werden.");
        }
        catch (Exception e)
        {
            var numberedException = new NumberedException(e);
            _logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
                nameof(ShoppingListController), nameof(UpdateShoppingList));
            return StatusCode(StatusCodes.Status500InternalServerError,
                "Aufgrund eines internen Fehlers konnte deine Anfrage nicht verarbeitet werden.");
        }
    }

    [HttpPatch]
    [Route("Item/{itemId:guid}")]
    public async Task<ActionResult> UpdateItem(Guid itemId, [FromBody] Model.Patch.ItemPatch itemPatch)
    {
        try
        {
            bool success = await _databaseService.SqlConnectionHandler<(Guid, Model.Patch.ItemPatch), bool>(
                (input, connection) => _databaseService.UpdateItem(input.Item1, input.Item2, connection),
                (itemId, itemPatch)
            );

            if (!success)
            {
                return NotFound($"Artikel mit ID {itemId} nicht gefunden");
            }

            return NoContent();
        }
        catch (NumberedException nEx)
        {
            _logger.LogWithLevel(LogLevel.Error, nEx, nEx.ErrorNumber, nEx.Message,
                nameof(ShoppingListController), nameof(UpdateItem));
            return StatusCode(StatusCodes.Status500InternalServerError,
                "Aufgrund eines internen Fehlers konnte deine Anfrage nicht verarbeitet werden.");
        }
        catch (Exception e)
        {
            var numberedException = new NumberedException(e);
            _logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
                nameof(ShoppingListController), nameof(UpdateItem));
            return StatusCode(StatusCodes.Status500InternalServerError,
                "Aufgrund eines internen Fehlers konnte deine Anfrage nicht verarbeitet werden.");
        }
    }

    [HttpPatch]
    [Route("Item/{itemId:guid}/purchase")]
    public async Task<ActionResult> MarkItemAsPurchased(Guid itemId, [FromBody] bool isPurchased)
    {
        try
        {
            bool success = await _databaseService.SqlConnectionHandler<(Guid, bool), bool>(
                (input, connection) => _databaseService.MarkItemAsPurchased(input.Item1, input.Item2, connection),
                (itemId, isPurchased)
            );

            if (!success)
            {
                return NotFound($"Artikel mit ID {itemId} nicht gefunden");
            }

            return NoContent();
        }
        catch (NumberedException nEx)
        {
            _logger.LogWithLevel(LogLevel.Error, nEx, nEx.ErrorNumber, nEx.Message,
                nameof(ShoppingListController), nameof(MarkItemAsPurchased));
            return StatusCode(StatusCodes.Status500InternalServerError,
                "Aufgrund eines internen Fehlers konnte deine Anfrage nicht verarbeitet werden.");
        }
        catch (Exception e)
        {
            var numberedException = new NumberedException(e);
            _logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
                nameof(ShoppingListController), nameof(MarkItemAsPurchased));
            return StatusCode(StatusCodes.Status500InternalServerError,
                "Aufgrund eines internen Fehlers konnte deine Anfrage nicht verarbeitet werden.");
        }
    }

    [HttpPatch]
    [Route("User/{userId:guid}/role")]
    public async Task<ActionResult> UpdateUserRoleInShoppingList(Guid userId, [FromQuery] Guid listId, [FromQuery] Guid roleId)
    {
        try
        {
            bool success = await _databaseService.SqlConnectionHandler<(Guid, Guid, Guid), bool>(
                (input, connection) => _databaseService.UpdateUserRoleInShoppingList(input.Item1, input.Item2, input.Item3, connection),
                (listId, userId, roleId)
            );

            if (!success)
            {
                return NotFound($"Benutzer mit ID {userId} in der Einkaufsliste mit ID {listId} nicht gefunden");
            }

            return NoContent();
        }
        catch (NumberedException nEx)
        {
            _logger.LogWithLevel(LogLevel.Error, nEx, nEx.ErrorNumber, nEx.Message,
                nameof(ShoppingListController), nameof(UpdateUserRoleInShoppingList));
            return StatusCode(StatusCodes.Status500InternalServerError,
                "Aufgrund eines internen Fehlers konnte deine Anfrage nicht verarbeitet werden.");
        }
        catch (Exception e)
        {
            var numberedException = new NumberedException(e);
            _logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
                nameof(ShoppingListController), nameof(UpdateUserRoleInShoppingList));
            return StatusCode(StatusCodes.Status500InternalServerError,
                "Aufgrund eines internen Fehlers konnte deine Anfrage nicht verarbeitet werden.");
        }
    }

    // DELETE-Methoden
    [HttpDelete]
    [Route("ShoppingList/{listId:guid}")]
    public async Task<ActionResult> DeleteShoppingList(Guid listId)
    {
        try
        {
            bool success = await _databaseService.SqlConnectionHandler<Guid, bool>(
                (input, connection) => _databaseService.DeleteShoppingList(input, connection),
                listId
            );

            if (!success)
            {
                return NotFound($"Einkaufsliste mit ID {listId} nicht gefunden");
            }

            return NoContent();
        }
        catch (NumberedException nEx)
        {
            _logger.LogWithLevel(LogLevel.Error, nEx, nEx.ErrorNumber, nEx.Message,
                nameof(ShoppingListController), nameof(DeleteShoppingList));
            return StatusCode(StatusCodes.Status500InternalServerError,
                "Aufgrund eines internen Fehlers konnte deine Anfrage nicht verarbeitet werden.");
        }
        catch (Exception e)
        {
            var numberedException = new NumberedException(e);
            _logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
                nameof(ShoppingListController), nameof(DeleteShoppingList));
            return StatusCode(StatusCodes.Status500InternalServerError,
                "Aufgrund eines internen Fehlers konnte deine Anfrage nicht verarbeitet werden.");
        }
    }

    [HttpDelete]
    [Route("Item/{itemId:guid}")]
    public async Task<ActionResult> DeleteItem(Guid itemId)
    {
        try
        {
            bool success = await _databaseService.SqlConnectionHandler<Guid, bool>(
                (input, connection) => _databaseService.DeleteItem(input, connection),
                itemId
            );

            if (!success)
            {
                return NotFound($"Artikel mit ID {itemId} nicht gefunden");
            }

            return NoContent();
        }
        catch (NumberedException nEx)
        {
            _logger.LogWithLevel(LogLevel.Error, nEx, nEx.ErrorNumber, nEx.Message,
                nameof(ShoppingListController), nameof(DeleteItem));
            return StatusCode(StatusCodes.Status500InternalServerError,
                "Aufgrund eines internen Fehlers konnte deine Anfrage nicht verarbeitet werden.");
        }
        catch (Exception e)
        {
            var numberedException = new NumberedException(e);
            _logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
                nameof(ShoppingListController), nameof(DeleteItem));
            return StatusCode(StatusCodes.Status500InternalServerError,
                "Aufgrund eines internen Fehlers konnte deine Anfrage nicht verarbeitet werden.");
        }
    }

    [HttpDelete]
    [Route("UserRole/{roleId:guid}")]
    public async Task<ActionResult> DeleteUserRole(Guid roleId)
    {
        try
        {
            bool success = await _databaseService.SqlConnectionHandler<Guid, bool>(
                (input, connection) => _databaseService.DeleteUserRole(input, connection),
                roleId
            );

            if (!success)
            {
                return NotFound($"Benutzerrolle mit ID {roleId} nicht gefunden");
            }

            return NoContent();
        }
        catch (NumberedException nEx)
        {
            _logger.LogWithLevel(LogLevel.Error, nEx, nEx.ErrorNumber, nEx.Message,
                nameof(ShoppingListController), nameof(DeleteUserRole));
            return StatusCode(StatusCodes.Status500InternalServerError,
                "Aufgrund eines internen Fehlers konnte deine Anfrage nicht verarbeitet werden.");
        }
        catch (Exception e)
        {
            var numberedException = new NumberedException(e);
            _logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
                nameof(ShoppingListController), nameof(DeleteUserRole));
            return StatusCode(StatusCodes.Status500InternalServerError,
                "Aufgrund eines internen Fehlers konnte deine Anfrage nicht verarbeitet werden.");
        }
    }

    [HttpDelete]
    [Route("ShoppingList/{listId:guid}/user/{userId:guid}")]
    public async Task<ActionResult> RemoveUserFromShoppingList(Guid listId, Guid userId)
    {
        try
        {
            bool success = await _databaseService.SqlConnectionHandler<(Guid, Guid), bool>(
                (input, connection) => _databaseService.RemoveUserFromShoppingList(input.Item1, input.Item2, connection),
                (listId, userId)
            );

            if (!success)
            {
                return NotFound($"Benutzer mit ID {userId} in der Einkaufsliste mit ID {listId} nicht gefunden");
            }

            return NoContent();
        }
        catch (NumberedException nEx)
        {
            _logger.LogWithLevel(LogLevel.Error, nEx, nEx.ErrorNumber, nEx.Message,
                nameof(ShoppingListController), nameof(RemoveUserFromShoppingList));
            return StatusCode(StatusCodes.Status500InternalServerError,
                "Aufgrund eines internen Fehlers konnte deine Anfrage nicht verarbeitet werden.");
        }
        catch (Exception e)
        {
            var numberedException = new NumberedException(e);
            _logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
                nameof(ShoppingListController), nameof(RemoveUserFromShoppingList));
            return StatusCode(StatusCodes.Status500InternalServerError,
                "Aufgrund eines internen Fehlers konnte deine Anfrage nicht verarbeitet werden.");
        }
    }

    [HttpPatch]
    [Route("{userId:guid}")]
    public async Task<ActionResult> UpdateUser(Guid userId, [FromBody] Model.Patch.ListUserPatch userPatch)
    {
        try
        {
            bool success = await _databaseService.SqlConnectionHandler<(Guid, Model.Patch.ListUserPatch), bool>(
                (input, connection) => _databaseService.UpdateUser(input.Item1, input.Item2, connection),
                (userId, userPatch)
            );

            if (!success)
            {
                return NotFound($"Benutzer mit ID {userId} nicht gefunden");
            }

            return NoContent();
        }
        catch (NumberedException nEx)
        {
            _logger.LogWithLevel(LogLevel.Error, nEx, nEx.ErrorNumber, nEx.Message,
                nameof(ShoppingListController), nameof(UpdateUser));
            return StatusCode(StatusCodes.Status500InternalServerError,
                "Aufgrund eines internen Fehlers konnte deine Anfrage nicht verarbeitet werden.");
        }
        catch (Exception e)
        {
            var numberedException = new NumberedException(e);
            _logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
                nameof(ShoppingListController), nameof(UpdateUser));
            return StatusCode(StatusCodes.Status500InternalServerError,
                "Aufgrund eines internen Fehlers konnte deine Anfrage nicht verarbeitet werden.");
        }
    }

    [HttpDelete]
    [Route("{userId:guid}")]
    public async Task<ActionResult> DeleteUser(Guid userId)
    {
        try
        {
            bool success = await _databaseService.SqlConnectionHandler<Guid, bool>(
                (input, connection) => _databaseService.DeleteUser(input, connection),
                userId
            );

            if (!success)
            {
                return NotFound($"Benutzer mit ID {userId} nicht gefunden");
            }

            return NoContent();
        }
        catch (NumberedException nEx)
        {
            _logger.LogWithLevel(LogLevel.Error, nEx, nEx.ErrorNumber, nEx.Message,
                nameof(ShoppingListController), nameof(DeleteUser));
            return StatusCode(StatusCodes.Status500InternalServerError,
                "Aufgrund eines internen Fehlers konnte deine Anfrage nicht verarbeitet werden.");
        }
        catch (Exception e)
        {
            var numberedException = new NumberedException(e);
            _logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
                nameof(ShoppingListController), nameof(DeleteUser));
            return StatusCode(StatusCodes.Status500InternalServerError,
                "Aufgrund eines internen Fehlers konnte deine Anfrage nicht verarbeitet werden.");
        }
    }
    */
}