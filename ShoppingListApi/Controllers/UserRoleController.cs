using Microsoft.AspNetCore.Mvc;
using ShoppingListApi.Attributes;
using ShoppingListApi.Interfaces.Services;
using ShoppingListApi.Model.DTOs.Get;
using ShoppingListApi.Model.DTOs.Patch;
using ShoppingListApi.Model.DTOs.Post;
using ShoppingListApi.Model.ReturnTypes;

namespace ShoppingListApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class UserRoleController(
        IUserRoleService userRoleService,
        IHostApplicationLifetime hostApplicationLifetime,
        ILogger<UserRoleController> logger)
        : ControllerBase
    {
        private readonly IUserRoleService _userRoleService = userRoleService;
        private readonly IHostApplicationLifetime _hostApplicationLifetime = hostApplicationLifetime;
        private readonly ILogger<UserRoleController> _logger = logger;

        /// <summary>
        /// [AdminEndpoint] - Retrieves a user role by its ID.
        /// Use this endpoint to get a user role by its ID as an admin.
        /// </summary>
        /// <param name="userRoleId">The ID of the user role to retrieve.</param>
        [HttpGet]
        [AdminEndpoint]
        [Route("{userRoleId:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserRoleGetDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ResponseResult<Guid>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResponseResult<object?>))]
        public async Task<ActionResult> GetUserRoleById([FromRoute] Guid userRoleId)
        {
            var ct = CancellationTokenSource
                .CreateLinkedTokenSource(_hostApplicationLifetime.ApplicationStopping, HttpContext.RequestAborted)
                .Token;

            var userRoleEntity = await _userRoleService.GetByIdAsync(userRoleId, ct);

            if (userRoleEntity is null)
                return NotFound(new ResponseResult<Guid>(userRoleId,
                    "UA user role for the provided id was not found."));

            return Ok(UserRoleGetDto.FromUserRole(userRoleEntity));
        }

        /// <summary>
        /// [PublicEndpoint] - Retrieves all user roles.
        /// Use this endpoint to get all user roles.
        /// </summary>
        [HttpGet]
        [Route("all")]
        [PublicEndpoint]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<UserRoleGetDto>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResponseResult<object?>))]
        public async Task<ActionResult> GetUserRoles()
        {
            var ct = CancellationTokenSource
                .CreateLinkedTokenSource(_hostApplicationLifetime.ApplicationStopping, HttpContext.RequestAborted)
                .Token;

            var userRoleEntities = await _userRoleService.GetAllAsync(ct);

            var userRoleDtoS = UserRoleGetDto.FromUserRoleList(userRoleEntities);

            if (userRoleDtoS.Count == 0)
            {
                return NoContent();
            }

            return Ok(userRoleDtoS);
        }


        /// <summary>
        /// [AdminEndpoint] - Creates a new user role.
        /// Use this endpoint to create a new user role as an admin.
        /// </summary>
        /// <param name="userRolePostDto">The user role details.</param>
        [HttpPost]
        [AdminEndpoint]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ResponseResult<Guid>))]
        [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ResponseResult<UserRoleGetDto>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResponseResult<UserRolePostDto>))]
        public async Task<ActionResult> CreateUserRole([FromBody] UserRolePostDto userRolePostDto)
        {
            var ct = CancellationTokenSource
                .CreateLinkedTokenSource(_hostApplicationLifetime.ApplicationStopping, HttpContext.RequestAborted)
                .Token;

            var result = await _userRoleService.CheckConflictAndAddUserRoleAsync(userRolePostDto, ct);

            if (result.Success is not true)
            {
                if (result is { Conflicts: true, ConflictingRecord: not null })
                {
                    return Conflict(new ResponseResult<UserRoleGetDto>(
                        UserRoleGetDto.FromUserRole(result.ConflictingRecord),
                        "Adding your input data would conflict with an existing record."));
                }

                return StatusCode(500,
                    new ResponseResult<UserRolePostDto>(userRolePostDto,
                        "Failed to create the user role due to an internal server error."));
            }

            return CreatedAtAction(nameof(GetUserRoleById), new { userRoleId = result.AddedRecord! },
                new ResponseResult<Guid>((Guid)result.AddedRecord!, "User role created successfully."));
        }


        /// <summary>
        /// [AdminEndpoint] - Updates an existing user role by its ID.
        /// Use this endpoint to update a user role by its ID as an admin.
        /// </summary>
        /// <param name="userRoleId">The ID of the user role to update.</param>
        /// <param name="userRolePatchDto">The new user role details.</param>
        [HttpPatch]
        [AdminEndpoint]
        [Route("{userRoleId:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseResult<Guid>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResponseResult<Guid>))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ResponseResult<Guid>))]
        [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ResponseResult<UserRoleGetDto>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError,
            Type = typeof(ResponseResult<UserRolePatchDto>))]
        public async Task<ActionResult> UpdateUserRole([FromRoute] Guid userRoleId,
            [FromBody] UserRolePatchDto userRolePatchDto)
        {
            var (userRoleTitle, userRoleEnum) = userRolePatchDto;

            if (userRoleTitle is null && userRoleEnum is null)
            {
                return BadRequest(new ResponseResult<Guid>(userRoleId, "No fields to update were provided."));
            }

            var ct = CancellationTokenSource
                .CreateLinkedTokenSource(_hostApplicationLifetime.ApplicationStopping, HttpContext.RequestAborted)
                .Token;

            var result = await _userRoleService.CheckConflictAndUpdateUserRoleAsync(userRoleId, userRolePatchDto, ct);

            if (result.Success is not true)
            {
                if (result is { TargetExists: false })
                {
                    return NotFound(new ResponseResult<Guid>(userRoleId,
                        "The user role to update was not found."));
                }

                if (result is { Conflicts: true, ConflictingRecord: not null })
                {
                    return Conflict(new ResponseResult<UserRoleGetDto>(
                        UserRoleGetDto.FromUserRole(result.ConflictingRecord),
                        "Updating your input data would conflict with an existing record."));
                }

                return StatusCode(500,
                    new ResponseResult<UserRolePatchDto>(userRolePatchDto,
                        "Failed to update the user role due to an internal server error."));
            }

            return Ok(new ResponseResult<Guid>(userRoleId, "User role updated successfully."));
        }
    }
}