using Microsoft.EntityFrameworkCore;
using ShoppingListApi.Data.Contexts;
using ShoppingListApi.Enums;
using ShoppingListApi.Interfaces.Repositories;
using ShoppingListApi.Model.DTOs.Patch;
using ShoppingListApi.Model.DTOs.Post;
using ShoppingListApi.Model.Entity;

namespace ShoppingListApi.Repositories;

public class UserRoleRepository(AppDbContext appDbContext) : IUserRoleRepository
{
    private readonly AppDbContext _appDbContext = appDbContext;

    /// <summary>
    /// Retrieves all UserRole entities from the database.
    /// </summary>
    public async Task<List<UserRole>> GetAllAsync(CancellationToken ct = default)
    {
        return await _appDbContext.UserRoles.ToListAsync(ct);
    }

    /// <summary>
    /// Retrieves a UserRole by its unique ID.
    /// Returns null if not found.
    /// </summary>
    public async Task<UserRole?> GetByIdAsync(Guid userRoleId, CancellationToken ct = default)
    {
        return await _appDbContext.UserRoles.FirstOrDefaultAsync(ur => ur.UserRoleId == userRoleId, ct);
    }

    /// <summary>
    /// Retrieves a UserRole by its enum value.
    /// Returns null if not found.
    /// </summary>
    public async Task<UserRole?> GetByEnumAsync(UserRoleEnum userRoleEnum, CancellationToken ct = default)
    {
        return await _appDbContext.UserRoles.FirstOrDefaultAsync(ur => ur.EnumIndex == (int)userRoleEnum, ct);
    }

    /// <summary>
    /// Retrieves a UserRole by its title string.
    /// Returns null if not found.
    /// </summary>
    public async Task<UserRole?> GetByTitleAsync(string userRoleTitle, CancellationToken ct = default)
    {
        return await _appDbContext.UserRoles.FirstOrDefaultAsync(ur => ur.UserRoleTitle == userRoleTitle, ct);
    }

    /// <summary>
    /// Adds a new UserRole to the database using the provided DTO and returns its ID.
    /// Does not save changes to the database.
    /// </summary>
    public async Task<Guid> AddAsync(UserRolePostDto userRolePostDto, CancellationToken ct = default)
    {
        var newUserRoleId = Guid.NewGuid();

        var newUserRole = new UserRole
        {
            UserRoleId = newUserRoleId,
            UserRoleTitle = userRolePostDto.UserRoleTitle,
            EnumIndex = (int)userRolePostDto.UserRoleEnum
        };

        await _appDbContext.UserRoles.AddAsync(newUserRole, ct);
        
        return newUserRoleId;
    }

    /// <summary>
    /// Updates the title and/or enum value of the specified UserRole using the provided patch DTO.
    /// </summary>
    public void Update(UserRole targetUserRole, UserRolePatchDto userRolePatchDto)
    {
        if (userRolePatchDto.UserRoleEnum is null && userRolePatchDto.UserRoleTitle is null)
            return;

        if (userRolePatchDto.UserRoleTitle is not null)
            targetUserRole.UserRoleTitle = userRolePatchDto.UserRoleTitle;
        if (userRolePatchDto.UserRoleEnum.HasValue)
            targetUserRole.EnumIndex = (int)userRolePatchDto.UserRoleEnum.Value;
    }
}