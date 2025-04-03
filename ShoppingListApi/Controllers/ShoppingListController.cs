using Microsoft.AspNetCore.Mvc;
using ShoppingListApi.Configs;
using ShoppingListApi.Exceptions;
using ShoppingListApi.Model.Get;
using ShoppingListApi.Model.Post;
using ShoppingListApi.Services;

namespace ShoppingListApi.Controllers;

[ApiController]
[Route("[controller]")]
public class ShoppingListController : ControllerBase
{
    private readonly DatabaseService _databaseService;
    private readonly ILogger<ShoppingListController> _logger;

    public ShoppingListController(IServiceProvider serviceProvider)
    {
        _databaseService = serviceProvider.GetRequiredService<DatabaseService>();
        _logger = serviceProvider.GetRequiredService<ILogger<ShoppingListController>>();
    }

    [HttpGet]
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
                nameof(ShoppingListController), nameof(GetUserRoles));
            return StatusCode(StatusCodes.Status500InternalServerError,
                "Du to an internal error, your request could not be processed.");
        }
        catch (Exception e)
        {
            var numberedException = new NumberedException(e);
            _logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
                nameof(ShoppingListController), nameof(GetUserRoles));
            return StatusCode(StatusCodes.Status500InternalServerError,
                "Du to an internal error, your request could not be processed.");
        }
    }

    [HttpPost]
    [Route("UserRole")]
    public async Task<ActionResult> AddUserRole([FromBody] UserRolePost userRolePost)
    {
        try
        {
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
                nameof(ShoppingListController), nameof(AddUserRole));
            return StatusCode(StatusCodes.Status500InternalServerError,
                "Du to an internal error, your request could not be processed.");
        }
        catch (Exception e)
        {
            var numberedException = new NumberedException(e);
            _logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
                nameof(ShoppingListController), nameof(AddUserRole));
            return StatusCode(StatusCodes.Status500InternalServerError,
                "Du to an internal error, your request could not be processed.");
        }
    }

    [HttpGet]
    [Route("by-email")]
    public async Task<ActionResult> GetUserByEmail([FromQuery] string email)
    {
        try
        {
            var user = await _databaseService.SqlConnectionHandler<string, ListUser?>(
                (input, connection) => _databaseService.GetUserByEmailAddress(input, connection),
                email
            );

            if (user == null)
            {
                return NotFound($"User with email {email} not found");
            }

            return Ok(user);
        }
        catch (NumberedException nEx)
        {
            _logger.LogWithLevel(LogLevel.Error, nEx, nEx.ErrorNumber, nEx.Message,
                nameof(ShoppingListController), nameof(GetUserByEmail));
            return StatusCode(StatusCodes.Status500InternalServerError,
                "Due to an internal error, your request could not be processed.");
        }
        catch (Exception e)
        {
            var numberedException = new NumberedException(e);
            _logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
                nameof(ShoppingListController), nameof(GetUserByEmail));
            return StatusCode(StatusCodes.Status500InternalServerError,
                "Due to an internal error, your request could not be processed.");
        }
    }

    [HttpGet]
    [Route("User/{userId:guid}")]
    public async Task<ActionResult> GetUserById(Guid userId)
    {
        try
        {
            var user = await _databaseService.SqlConnectionHandler<Guid, ListUser?>(
                (input, connection) => _databaseService.GetUserById(input, connection),
                userId
            );

            if (user == null)
            {
                return NotFound($"User with ID {userId} not found");
            }

            return Ok(user);
        }
        catch (NumberedException nEx)
        {
            _logger.LogWithLevel(LogLevel.Error, nEx, nEx.ErrorNumber, nEx.Message,
                nameof(ShoppingListController), nameof(GetUserById));
            return StatusCode(StatusCodes.Status500InternalServerError,
                "Due to an internal error, your request could not be processed.");
        }
        catch (Exception e)
        {
            var numberedException = new NumberedException(e);
            _logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
                nameof(ShoppingListController), nameof(GetUserById));
            return StatusCode(StatusCodes.Status500InternalServerError,
                "Due to an internal error, your request could not be processed.");
        }
    }

    [HttpGet]
    [Route("ShoppingList/{listId:guid}")]
    public async Task<ActionResult> GetShoppingList(Guid listId)
    {
        try
        {
            var shoppingList = await _databaseService.SqlConnectionHandler<Guid, ShoppingList?>(
                (input, connection) => _databaseService.GetShoppingListById(input, connection),
                listId
            );

            if (shoppingList == null)
            {
                return NotFound($"Shopping list with ID {listId} not found");
            }

            return Ok(shoppingList);
        }
        catch (NumberedException nEx)
        {
            _logger.LogWithLevel(LogLevel.Error, nEx, nEx.ErrorNumber, nEx.Message,
                nameof(ShoppingListController), nameof(GetShoppingList));
            return StatusCode(StatusCodes.Status500InternalServerError,
                "Due to an internal error, your request could not be processed.");
        }
        catch (Exception e)
        {
            var numberedException = new NumberedException(e);
            _logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
                nameof(ShoppingListController), nameof(GetShoppingList));
            return StatusCode(StatusCodes.Status500InternalServerError,
                "Due to an internal error, your request could not be processed.");
        }
    }

    [HttpGet]
    [Route("User/{userId:guid}/lists")]
    public async Task<ActionResult> GetUserShoppingLists(Guid userId)
    {
        try
        {
            var shoppingLists = await _databaseService.SqlConnectionHandler<Guid, List<ShoppingList>>(
                (input, connection) => _databaseService.GetShoppingListsForUser(input, connection),
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
                nameof(ShoppingListController), nameof(GetUserShoppingLists));
            return StatusCode(StatusCodes.Status500InternalServerError,
                "Due to an internal error, your request could not be processed.");
        }
        catch (Exception e)
        {
            var numberedException = new NumberedException(e);
            _logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
                nameof(ShoppingListController), nameof(GetUserShoppingLists));
            return StatusCode(StatusCodes.Status500InternalServerError,
                "Due to an internal error, your request could not be processed.");
        }
    }

    [HttpGet]
    [Route("ShoppingList/{listId:guid}/items")]
    public async Task<ActionResult> GetShoppingListItems(Guid listId)
    {
        try
        {
            var items = await _databaseService.SqlConnectionHandler<Guid, List<Item>>(
                (input, connection) => _databaseService.GetItemsForShoppingList(input, connection),
                listId
            );

            if (items.Count == 0)
            {
                return NoContent();
            }

            return Ok(items);
        }
        catch (NumberedException nEx)
        {
            _logger.LogWithLevel(LogLevel.Error, nEx, nEx.ErrorNumber, nEx.Message,
                nameof(ShoppingListController), nameof(GetShoppingListItems));
            return StatusCode(StatusCodes.Status500InternalServerError,
                "Due to an internal error, your request could not be processed.");
        }
        catch (Exception e)
        {
            var numberedException = new NumberedException(e);
            _logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
                nameof(ShoppingListController), nameof(GetShoppingListItems));
            return StatusCode(StatusCodes.Status500InternalServerError,
                "Due to an internal error, your request could not be processed.");
        }
    }
    
    // PATCH-Methoden
    [HttpPatch]
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
}