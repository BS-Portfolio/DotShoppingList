using ShoppingListApi.Enums;
using ShoppingListApi.Interfaces.Repositories;
using ShoppingListApi.Interfaces.Services;
using ShoppingListApi.Model.DTOs.Patch;
using ShoppingListApi.Model.DTOs.Post;
using ShoppingListApi.Model.Entity;
using ShoppingListApi.Model.ReturnTypes;

namespace ShoppingListApi.Services;

public class UserRoleService(IUserRoleRepository userRoleRepository, ILogger<UserRoleService> logger) : IUserRoleService
{
    private readonly IUserRoleRepository _userRoleRepository = userRoleRepository;
    private readonly ILogger<UserRoleService> _logger = logger;

    public IUserRoleRepository USerRoleRepository => _userRoleRepository;

    public async Task<AddRecordResult<Guid?, UserRole?>> CheckConflictAndAddUserRoleAsync(
        UserRolePostDto userRolePostDto, CancellationToken ct = default)
    {
        try
        {
            var existingRoleByEnum = await _userRoleRepository.GetByEnumAsync(userRolePostDto.UserRoleEnum, ct);
            if (existingRoleByEnum is not null)
                return new(false, null, true, existingRoleByEnum);

            var existingRoleByTitle = await _userRoleRepository.GetByTitleAsync(userRolePostDto.UserRoleTitle, ct);
            if (existingRoleByTitle is not null)
                return new(false, null, true, existingRoleByTitle);

            var addedUserRoleId = await _userRoleRepository.AddAsync(userRolePostDto, ct);

            if (addedUserRoleId is null)
                return new(false, null, false, null);

            return new(true, addedUserRoleId, false, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding user role");
            throw;
        }
    }

    public async Task<UpdateRecordResult<UserRole?>> CheckConflictAndUpdateUserRoleAsync(
        Guid userRoleId, UserRolePatchDto userRolePatchDto, CancellationToken ct = default)
    {
        try
        {
            var targetUserRole = await _userRoleRepository.GetByIdAsync(userRoleId, ct);
            if (targetUserRole is null)
                return new(false, false, false, null);

            if (userRolePatchDto.UserRoleEnum is not null)
            {
                var existingRoleByEnum =
                    await _userRoleRepository.GetByEnumAsync((UserRoleEnum)userRolePatchDto.UserRoleEnum, ct);
                if (existingRoleByEnum is not null && existingRoleByEnum.UserRoleId != userRoleId)
                    return new(true, false, true, existingRoleByEnum);
            }

            if (userRolePatchDto.UserRoleTitle is not null)
            {
                var existingRoleByTitle = await _userRoleRepository.GetByTitleAsync(userRolePatchDto.UserRoleTitle, ct);
                if (existingRoleByTitle is not null && existingRoleByTitle.UserRoleId != userRoleId)
                    return new(true, false, true, existingRoleByTitle);
            }

            var updatedUserRole = await _userRoleRepository.UpdateAsync(userRoleId, userRolePatchDto, ct);

            if (updatedUserRole is false)
                return new(true, false, false, null);

            return new(true, true, false, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user role");
            throw;
        }
    }
}