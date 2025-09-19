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

    public async Task<List<UserRole>> GetAllAsync(CancellationToken ct = default)
    {
        return await _appDbContext.UserRoles.ToListAsync(ct);
    }

    public async Task<UserRole?> GetByIdAsync(Guid userRoleId, CancellationToken ct = default)
    {
        return await _appDbContext.UserRoles.FirstOrDefaultAsync(ur => ur.UserRoleId == userRoleId, ct);
    }

    public async Task<UserRole?> GetByEnumAsync(UserRoleEnum userRoleEnum, CancellationToken ct = default)
    {
        return await _appDbContext.UserRoles.FirstOrDefaultAsync(ur => ur.EnumIndex == (int)userRoleEnum, ct);
    }

    public async Task<UserRole?> GetByTitleAsync(string userRoleTitle, CancellationToken ct = default)
    {
        return await _appDbContext.UserRoles.FirstOrDefaultAsync(ur => ur.UserRoleTitle == userRoleTitle, ct);
    }

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