using Microsoft.AspNetCore.Mvc;
using ShoppingListApi.Attributes;
using ShoppingListApi.Exceptions;
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
        private readonly ILogger<UserRoleController> _logger = logger;
        private readonly IHostApplicationLifetime _hostApplicationLifetime = hostApplicationLifetime;

        [HttpGet]
        [AdminEndpoint]
        [Route("{userRoleId:guid}")]
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

        [HttpGet]
        [Route("all")]
        [PublicEndpoint]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<UserRoleGetDto>))]
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


        [HttpPost]
        [AdminEndpoint]
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


        [HttpPatch]
        [AdminEndpoint]
        [Route("{userRoleId:guid}")]
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