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

        try
        {
            var shoppingLists =
                await _databaseServiceObsolete.SqlConnectionHandlerAsync<Guid, List<ShoppingListGetDto>>(
                    async (id, connection) =>
                        await _databaseServiceObsolete.HandleShoppingListsFetchForUserAsync(id, connection),
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

        try
        {
            var result = await _databaseServiceObsolete
                .SqlConnectionHandlerAsync<ShoppingListIdentificationData,
                    FetchRestrictedRecordResult<ShoppingListGetDto?>>(
                    async (input, connection) =>
                        await _databaseServiceObsolete.HandleShoppingListFetchForUserAsync(input, connection),
                    new ShoppingListIdentificationData((Guid)requestingUserId, shoppingListId));

            if (result.Record is null)
            {
                if (result.RecordExists is false)
                {
                    return NotFound(
                        "A shopping list for the provided list owner and shopping list id's does not exists!");
                }

                if (result.AccessGranted is false)
                {
                    return Unauthorized(new AuthenticationErrorResponse(AuthorizationErrorEnum.ListAccessNotGranted));
                }

                return Problem("Due to an internal error, your request could not be processed.");
            }

            return Ok(result.Record);
        }
        catch (NumberedException nEx)
        {
            _logger.LogWithLevel(LogLevel.Error, nEx, nEx.ErrorNumber, nEx.Message,
                nameof(ShoppingListApiController), nameof(GetShoppingListForUser));
            return StatusCode(StatusCodes.Status500InternalServerError,
                "Due to an internal error, your request could not be processed.");
        }
        catch (Exception e)
        {
            var numberedException = new NumberedException(e);
            _logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
                nameof(ShoppingListApiController), nameof(GetShoppingListForUser));
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
    /// public endpoint to register new users
    /// </summary>
    /// <param name="userPostDto"></param>
    /// <returns></returns>
    [HttpPost]
    [PublicEndpoint]
    [ProducesResponseType<string>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<Guid>(StatusCodes.Status201Created)]
    [ProducesResponseType<string>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<string>(StatusCodes.Status500InternalServerError)]
    [Route("User")]
    public async Task<ActionResult> AddNewUser([FromBody] ListUserPostDto userPostDto)
    {
        try
        {
            var userPostExtended = new ListUserPostExtendedDto(userPostDto);

            var emailAlreadyExists = await _databaseServiceObsolete.SqlConnectionHandlerAsync<string, bool>(
                async (emailAddress, sqlConnection) =>
                    await _databaseServiceObsolete.CheckUserExistenceAsync(emailAddress, sqlConnection),
                userPostExtended.EmailAddress);

            if (emailAlreadyExists)
            {
                return Conflict(
                    "A user with the same email address already exists in the database. Use a different address to register yourself!");
            }

            var (success, addedUserId) =
                await _databaseServiceObsolete.SqlConnectionHandlerAsync<ListUserPostExtendedDto, (bool, Guid?)>(
                    (input, connection) => _databaseServiceObsolete.AddUserAsync(input, connection), userPostExtended);

            if (success is false || addedUserId is null)
            {
                return Problem("Due to an internal error, you request could not be processed.");
            }

            return Ok(addedUserId);
        }
        catch (FormatException fEx)
        {
            _logger.LogWithLevel(LogLevel.Error, fEx, "0", fEx.Message,
                nameof(ShoppingListApiController), nameof(AddNewUser));
            return BadRequest("Your input email address and password were delivered in wrong formatting!");
        }
        catch (NumberedException nEx)
        {
            _logger.LogWithLevel(LogLevel.Error, nEx, nEx.ErrorNumber, nEx.Message,
                nameof(ShoppingListApiController), nameof(AddNewUser));
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

    /// <summary>
    /// public endpoint to log in
    /// </summary>
    /// <param name="loginDataDto"></param>
    /// <returns></returns>
    [HttpPost]
    [Route("User/Login")]
    [PublicEndpoint]
    [ProducesResponseType<ListUserGetDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<AuthenticationErrorResponse>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<string>(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> UserLogin([FromBody] LoginDataDto loginDataDto)
    {
        try
        {
            var user = await _databaseServiceObsolete.SqlConnectionHandlerAsync<LoginDataDto, ListUserGetDto?>(
                async (input, sqlConnection) => await _databaseServiceObsolete.HandleLoginAsync(input, sqlConnection),
                loginDataDto);

            if (user is null)
            {
                return Unauthorized(new AuthenticationErrorResponse(AuthorizationErrorEnum.LoginFailure));
            }

            return Ok(user);
        }
        catch (FormatException fEx)
        {
            _logger.LogWithLevel(LogLevel.Error, fEx, "0", fEx.Message,
                nameof(ShoppingListApiController), nameof(UserLogin));
            return BadRequest("Your input email address and password were delivered in wrong formatting!");
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

    /// <summary>
    /// user endpoint to add a shopping list for a user. The max allowed amount of shopping lists for every user as list owner is 5. The variable userId points to the list owner ID.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="shoppingListName"></param>
    /// <param name="requestingUserId"></param>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType<Guid>(StatusCodes.Status200OK)]
    [ProducesResponseType<string>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<string>(StatusCodes.Status500InternalServerError)]
    [Route("User/{userId:Guid}/ShoppingList")]
    public async Task<ActionResult> AddShoppingListForUser([FromRoute] Guid userId, [FromBody] string shoppingListName,
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

        try
        {
            var result = await _databaseServiceObsolete
                .SqlConnectionHandlerAsync<ShoppingListAdditionData, ShoppingListAdditionResult>(
                    async (data, sqlConnection) =>
                        await _databaseServiceObsolete.HandleAddingShoppingListAsync(data, sqlConnection),
                    new ShoppingListAdditionData(shoppingListName, userId));

            if (result.Success is false || result.AddedShoppingListId is null)
            {
                if (result.NameAlreadyExists is true)
                {
                    return Conflict($"You already have a shopping list named: \"{shoppingListName}\"");
                }

                if (result.MaximumNumberOfListsReached is true)
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
                        await _databaseServiceObsolete.SqlConnectionHandlerAsync<Guid, bool>(
                            async (input, connection) =>
                                await _databaseServiceObsolete.RemoveShoppingListByIdAsync(input, connection),
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

        try
        {
            var result = await _databaseServiceObsolete.SqlConnectionHandlerAsync<NewItemData, ItemAdditionResult>(
                async (data, connection) =>
                    await _databaseServiceObsolete.HandleAddingItemToShoppingListAsync(data, connection),
                new NewItemData(itemPostDto, shoppingListId, userId, requestingUserId));

            if (result.Success is false || result.ItemId is null)
            {
                if (result.AccessGranted is false)
                {
                    return Unauthorized(new AuthenticationErrorResponse(AuthorizationErrorEnum.ListAccessNotGranted));
                }

                if (result.MaximumCountReached is true)
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
    public async Task<ActionResult> AddCollaboratorToShoppingList([FromHeader(Name = "USER-ID")] Guid? requestingUserId,
        Guid userId, Guid shoppingListId, string collaboratorEmailAddress)
    {
        try
        {
            if (requestingUserId is null)
            {
                return Unauthorized(new AuthenticationErrorResponse(AuthorizationErrorEnum.UserCredentialsMissing));
            }

            var result = await _databaseServiceObsolete
                .SqlConnectionHandlerAsync<CollaboratorAdditionData, CollaboratorAddRemoveResult>(
                    async (input, connection) =>
                        await _databaseServiceObsolete.HandleAddingCollaboratorToShoppingListAsync(input, connection),
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
    /// user endpoint to modify the name of the shopping list by list owner. The variable userId points to the list owner ID.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="shoppingListId"></param>
    /// <param name="shoppingListPatchDtoObsolete"></param>
    /// <param name="requestingUserId"></param>
    /// <returns></returns>
    [HttpPatch]
    [ProducesResponseType<string>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<AuthenticationErrorResponse>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<string>(StatusCodes.Status200OK)]
    [ProducesResponseType<string>(StatusCodes.Status500InternalServerError)]
    [Route("User/{userId:Guid}/ShoppingList/{shoppingListId:Guid}")]
    public async Task<ActionResult> ModifyShoppingListName([FromRoute] Guid userId, [FromRoute] Guid shoppingListId,
        [FromBody] ShoppingListPatchDtoObsolete shoppingListPatchDtoObsolete,
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

        try
        {
            var result = await _databaseServiceObsolete
                .SqlConnectionHandlerAsync<
                    ModificationData<(Guid userId, Guid shoppingListId), ShoppingListPatchDtoObsolete>,
                    UpdateResult>(
                    (input, connection) => _databaseServiceObsolete.HandleShoppingListNameUpdateAsync(input, connection)
                    , new ModificationData<(Guid userId, Guid shoppingListId), ShoppingListPatchDtoObsolete>(((Guid)requestingUserId, shoppingListId), shoppingListPatchDtoObsolete));

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

    /// <summary>
    /// user endpoint to modify the details of an item in the shopping list. The variable userId points to the list owner ID.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="shoppingListId"></param>
    /// <param name="itemId"></param>
    /// <param name="itemPatchDtoObsolete"></param>
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
        ItemPatchDtoObsolete itemPatchDtoObsolete, [FromHeader(Name = "USER-ID")] Guid? requestingUserId)
    {
        if (requestingUserId is null)
        {
            return Unauthorized(new AuthenticationErrorResponse(AuthorizationErrorEnum.UserCredentialsMissing));
        }

        if (itemPatchDtoObsolete.NewItemAmount is null &&
            itemPatchDtoObsolete.NewItemName is null)
        {
            return BadRequest("Nothing to update!");
        }

        try
        {
            var result = await _databaseServiceObsolete
                .SqlConnectionHandlerAsync<ModificationData<(Guid userId, Guid shoppingListId, Guid itemId),
                        ItemPatchDtoObsolete>,
                    UpdateResult>(
                    (input, connection) => _databaseServiceObsolete.HandleShoppingListItemUpdateAsync(input, connection)
                    , new ModificationData<(Guid userId, Guid shoppingListId, Guid itemId), ItemPatchDtoObsolete>(((Guid)requestingUserId, shoppingListId, itemId), itemPatchDtoObsolete));

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
        {
            return Unauthorized(new AuthenticationErrorResponse(AuthorizationErrorEnum.UserCredentialsMissing));
        }

        if (!requestingUserId.Equals(userId))
        {
            return Unauthorized(new AuthenticationErrorResponse(AuthorizationErrorEnum.ListAccessNotGranted));
        }

        try
        {
            var result = await _databaseServiceObsolete
                .SqlConnectionHandlerAsync<ShoppingListIdentificationData, ShoppingListRemovalResult>(
                    async (input, connection) =>
                        await _databaseServiceObsolete.HandleShoppingListRemovalAsync(input, connection)
                    , new ShoppingListIdentificationData((Guid)requestingUserId, shoppingListId));

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
    public async Task<ActionResult> RemoveItemFromShoppingList([FromRoute] Guid userId, [FromRoute] Guid shoppingListId,
        [FromRoute] Guid itemId, [FromHeader(Name = "USER-ID")] Guid? requestingUserId)
    {
        if (requestingUserId is null)
        {
            return Unauthorized(new AuthenticationErrorResponse(AuthorizationErrorEnum.UserCredentialsMissing));
        }

        try
        {
            var result = await _databaseServiceObsolete.SqlConnectionHandlerAsync<ItemIdentificationData, UpdateResult>(
                async (input, connection) =>
                    await _databaseServiceObsolete.HandleItemRemovalFromShoppingListAsync(input, connection)
                , new ItemIdentificationData((Guid)requestingUserId, shoppingListId, itemId));

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
                _databaseServiceObsolete
                    .SqlConnectionHandlerAsync<CollaboratorRemovalCheck, CollaboratorAddRemoveResult>(
                        async (input, connection) =>
                            await _databaseServiceObsolete.HandleCollaboratorRemovalAsync(input, connection),
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