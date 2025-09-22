using Microsoft.AspNetCore.Mvc;
using ShoppingListApi.Attributes;
using ShoppingListApi.Configs;
using ShoppingListApi.Enums;
using ShoppingListApi.Exceptions;
using ShoppingListApi.Model.Database;
using ShoppingListApi.Model.DTOs.Get;
using ShoppingListApi.Model.DTOs.PatchObsolete;
using ShoppingListApi.Model.DTOs.ObsoletePost;
using ShoppingListApi.Model.ReturnTypes;
using ShoppingListApi.Services;

namespace ShoppingListApi.Controllers;

[ApiController]
[Route("[controller]")]
public class ShoppingListApiController : ControllerBase
{
    private readonly DatabaseServiceObsolete _databaseServiceObsolete;
    private readonly ILogger<ShoppingListApiController> _logger;

    public ShoppingListApiController(IServiceProvider serviceProvider)
    {
        _databaseServiceObsolete = serviceProvider.GetRequiredService<DatabaseServiceObsolete>();
        _logger = serviceProvider.GetRequiredService<ILogger<ShoppingListApiController>>();
    }

    

    /// <summary>
    /// admin endpoint to get user details by email address for an api admin
    /// </summary>
    /// <param name="emailAddress"></param>
    /// <returns></returns>
    [HttpGet]
    [AdminEndpoint]
    [ProducesResponseType<string>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ListUserGetDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<string>(StatusCodes.Status500InternalServerError)]
    [Route("User/EmailAddress/{emailAddress}")]
    public async Task<ActionResult> GetUserByEmail([FromRoute] string emailAddress)
    {
        try
        {
            var user = await _databaseServiceObsolete.SqlConnectionHandlerAsync<string, ListUserGetDto?>(
                (input, connection) => _databaseServiceObsolete.GetUserByEmailAddressAsync(input, connection),
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

  

    

    /// <summary>
    /// admin endpoint to retrieve a user by their id
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    [HttpGet]
    [AdminEndpoint]
    [ProducesResponseType<string>(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType<ListUserGetDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<string>(StatusCodes.Status404NotFound)]
    [Route("User/{userId:Guid}")]
    public async Task<ActionResult> GetUserById(Guid userId)
    {
        try
        {
            var user = await _databaseServiceObsolete.SqlConnectionHandlerAsync<Guid, ListUserGetDto?>(
                (input, connection) => _databaseServiceObsolete.GetUserByIdAsync(input, connection),
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

    /// <summary>
    /// admin endpoint to retrieve a list the minimal data of all the registered users 
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [AdminEndpoint]
    [ProducesResponseType<List<ListUserMinimalGetDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<string>(StatusCodes.Status500InternalServerError)]
    [Route("User/all")]
    public async Task<ActionResult> GetAllUsers()
    {
        try
        {
            var users =
                await _databaseServiceObsolete.SqlConnectionHandlerAsync<List<ListUserMinimalGetDto>>(async
                    connection => await _databaseServiceObsolete.GetAllUsersAsync(connection));

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

    /// <summary>
    /// user endpoint to modify the first and last name.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="listUserPatchDtoObsolete"></param>
    /// <param name="requestingUserId"></param>
    /// <returns></returns>
    [HttpPatch]
    [ProducesResponseType<string>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<string>(StatusCodes.Status200OK)]
    [ProducesResponseType<string>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<string>(StatusCodes.Status500InternalServerError)]
    [Route("User/{userId:Guid}")]
    public async Task<ActionResult> ModifyUserDetails([FromRoute] Guid userId,
        [FromBody] ListUserPatchDtoObsolete listUserPatchDtoObsolete,
        [FromHeader(Name = "USER-ID")] Guid? requestingUserId)
    {
        if (requestingUserId is null)
        {
            return Unauthorized(new AuthenticationErrorResponse(AuthorizationErrorEnum.UserCredentialsMissing));
        }

        if (!userId.Equals(requestingUserId))
        {
            return Unauthorized(new AuthenticationErrorResponse(AuthorizationErrorEnum.ListAccessNotGranted));
        }

        if (listUserPatchDtoObsolete.NewFirstName is null &&
            listUserPatchDtoObsolete.NewLastName is null)
        {
            return BadRequest("Nothing to update!");
        }

        try
        {
            var success = await _databaseServiceObsolete
                .SqlConnectionHandlerAsync<ModificationData<Guid, ListUserPatchDtoObsolete>, bool>(
                    async (input, connection) =>
                        await _databaseServiceObsolete.ModifyUserDetailsAsync(input, connection),
                    new ModificationData<Guid, ListUserPatchDtoObsolete>(userId, listUserPatchDtoObsolete));

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

    /// <summary>
    /// admin endpoint to remove all the data of a user from the database via their email address.
    /// </summary>
    /// <param name="emailAddress"></param>
    /// <returns></returns>
    [HttpDelete]
    [AdminEndpoint]
    [ProducesResponseType<string>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<int>(StatusCodes.Status200OK)]
    [ProducesResponseType<string>(StatusCodes.Status500InternalServerError)]
    [Route("User")]
    public async Task<ActionResult> RemoveUserByEmailAddress([FromQuery] string emailAddress)
    {
        try
        {
            var result = await _databaseServiceObsolete
                .SqlConnectionHandlerAsync<string, UserRemovalDbResult>(
                    async (input, connection) =>
                        await _databaseServiceObsolete.RemoveUserByEmailAsync(input, connection)
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
  
    
    

    



}