using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ShoppingListApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {

        // [HttpPost]
        // [Route("register")]
        // public async Task<ActionResult> Register([FromBody] RegisterDataDto registerDataDto)
        // {
        //     var newUser = await _appAuthenticationService.Register(registerDataDto);
        //     
        // }


        /// <summary>
        /// public endpoint to register new users
        /// </summary>
        /// <param name="userPostDto"></param>
        /// <returns></returns>
        // [HttpPost]
        // [PublicEndpoint]
        // [ProducesResponseType<string>(StatusCodes.Status409Conflict)]
        // [ProducesResponseType<Guid>(StatusCodes.Status201Created)]
        // [ProducesResponseType<string>(StatusCodes.Status400BadRequest)]
        // [ProducesResponseType<string>(StatusCodes.Status500InternalServerError)]
        // [Route("User")]
        // public async Task<ActionResult> AddNewUser([FromBody] ListUserPostDto userPostDto)
        // {
        //     try
        //     {
        //         var userPostExtended = new ListUserPostExtendedDto(userPostDto);
        //
        //         var emailAlreadyExists = await _databaseServiceObsolete.SqlConnectionHandlerAsync<string, bool>(
        //             async (emailAddress, sqlConnection) =>
        //                 await _databaseServiceObsolete.CheckUserExistenceAsync(emailAddress, sqlConnection),
        //             userPostExtended.EmailAddress);
        //
        //         if (emailAlreadyExists)
        //         {
        //             return Conflict(
        //                 "A user with the same email address already exists in the database. Use a different address to register yourself!");
        //         }
        //
        //         var (success, addedUserId) =
        //             await _databaseServiceObsolete.SqlConnectionHandlerAsync<ListUserPostExtendedDto, (bool, Guid?)>(
        //                 (input, connection) => _databaseServiceObsolete.AddUserAsync(input, connection),
        //                 userPostExtended);
        //
        //         if (success is false || addedUserId is null)
        //         {
        //             return Problem("Due to an internal error, you request could not be processed.");
        //         }
        //
        //         return Ok(addedUserId);
        //     }
        //     catch (FormatException fEx)
        //     {
        //         _logger.LogWithLevel(LogLevel.Error, fEx, "0", fEx.Message,
        //             nameof(ShoppingListApiController), nameof(AddNewUser));
        //         return BadRequest("Your input email address and password were delivered in wrong formatting!");
        //     }
        //     catch (NumberedException nEx)
        //     {
        //         _logger.LogWithLevel(LogLevel.Error, nEx, nEx.ErrorNumber, nEx.Message,
        //             nameof(ShoppingListApiController), nameof(AddNewUser));
        //         return StatusCode(StatusCodes.Status500InternalServerError,
        //             "Due to an internal error, your request could not be processed.");
        //     }
        //     catch (Exception e)
        //     {
        //         var numberedException = new NumberedException(e);
        //         _logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
        //             nameof(ShoppingListApiController), nameof(AddNewUser));
        //         return StatusCode(StatusCodes.Status500InternalServerError,
        //             "Due to an internal error, your request could not be processed.");
        //     }
        // }
    }
}
