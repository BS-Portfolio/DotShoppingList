using ShoppingListApi.Configs;
using ShoppingListApi.Enums;
using ShoppingListApi.Exceptions;
using ShoppingListApi.Interfaces.Services;
using ShoppingListApi.Model.DTOs.Patch;
using ShoppingListApi.Model.DTOs.Post;
using ShoppingListApi.Model.Entity;
using ShoppingListApi.Model.ReturnTypes;

namespace ShoppingListApi.Services;

public class UserRoleService(IUnitOfWork unitOfWork, ILogger<UserRoleService> logger) : IUserRoleService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ILogger<UserRoleService> _logger = logger;


    public async Task<UserRole?> GetOwnerUserRole(CancellationToken ct = default)
    {
        try
        {
            return await _unitOfWork.UserRoleRepository.GetByEnumAsync(UserRoleEnum.ListOwner, ct);
        }
        catch (Exception e)
        {
            var numberedException = new NumberedException(e);
            _logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
                nameof(UserRoleService), nameof(GetOwnerUserRole));
            throw;
        }
    }

    public async Task<UserRole?> GetCollaboratorUserRole(CancellationToken ct = default)
    {
        try
        {
            return await _unitOfWork.UserRoleRepository.GetByEnumAsync(UserRoleEnum.Collaborator, ct);
        }
        catch (Exception e)
        {
            var numberedException = new NumberedException(e);
            _logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
                nameof(UserRoleService), nameof(GetCollaboratorUserRole));
            throw;
        }
    }

    public async Task<AddRecordResult<Guid?, UserRole?>> CheckConflictAndAddUserRoleAsync(
        UserRolePostDto userRolePostDto, CancellationToken ct = default)
    {
        try
        {
            var existingRoleByEnum =
                await _unitOfWork.UserRoleRepository.GetByEnumAsync(userRolePostDto.UserRoleEnum, ct);
            if (existingRoleByEnum is not null)
                return new(false, null, true, existingRoleByEnum);

            var existingRoleByTitle =
                await _unitOfWork.UserRoleRepository.GetByTitleAsync(userRolePostDto.UserRoleTitle, ct);
            if (existingRoleByTitle is not null)
                return new(false, null, true, existingRoleByTitle);

            var addedUserRoleId = await _unitOfWork.UserRoleRepository.AddAsync(userRolePostDto, ct);

            var checkResult = await _unitOfWork.SaveChangesAsync(ct);

            if (checkResult != 1)
                return new(false, null, false, null);

            return new(true, addedUserRoleId, false, null);
        }
        catch (Exception e)
        {
            var numberedException = new NumberedException(e);
            _logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
                nameof(UserRoleService), nameof(CheckConflictAndAddUserRoleAsync));
            throw;
        }
    }

    public async Task<UpdateRecordResult<UserRole?>> CheckConflictAndUpdateUserRoleAsync(
        Guid userRoleId, UserRolePatchDto userRolePatchDto, CancellationToken ct = default)
    {
        try
        {
            var targetUserRole = await _unitOfWork.UserRoleRepository.GetByIdAsync(userRoleId, ct);
            if (targetUserRole is null)
                return new(false, false, false, null);

            if (userRolePatchDto.UserRoleEnum is not null)
            {
                var existingRoleByEnum =
                    await _unitOfWork.UserRoleRepository.GetByEnumAsync(
                        (UserRoleEnum)userRolePatchDto.UserRoleEnum, ct);
                if (existingRoleByEnum is not null && existingRoleByEnum.UserRoleId != userRoleId)
                    return new(true, false, true, existingRoleByEnum);
            }

            if (userRolePatchDto.UserRoleTitle is not null)
            {
                var existingRoleByTitle =
                    await _unitOfWork.UserRoleRepository.GetByTitleAsync(userRolePatchDto.UserRoleTitle, ct);
                if (existingRoleByTitle is not null && existingRoleByTitle.UserRoleId != userRoleId)
                    return new(true, false, true, existingRoleByTitle);
            }

            _unitOfWork.UserRoleRepository.Update(targetUserRole, userRolePatchDto);

            var checkResult = await _unitOfWork.SaveChangesAsync(ct);

            if (checkResult != 1)
                return new(true, false, false, null);

            return new(true, true, false, null);
        }
        catch (Exception e)
        {
            var numberedException = new NumberedException(e);
            _logger.LogWithLevel(LogLevel.Error, e, numberedException.ErrorNumber, numberedException.Message,
                nameof(UserRoleService), nameof(CheckConflictAndUpdateUserRoleAsync));
            throw;
        }
    }
}