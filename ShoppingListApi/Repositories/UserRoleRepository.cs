using Microsoft.EntityFrameworkCore;
using ShoppingListApi.Data.Contexts;
using ShoppingListApi.Enums;
using ShoppingListApi.Interfaces.Repositories;
using ShoppingListApi.Model.DTOs.PatchObsolete;
using ShoppingListApi.Model.DTOs.Post;
using ShoppingListApi.Model.Entity;

namespace ShoppingListApi.Repositories;

public class UserRoleRepository(AppDbContext appDbContext, ILogger<UserRoleRepository> logger) : IUserRoleRepository
{
    private readonly ILogger<UserRoleRepository> _logger = logger;
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
    
    public async Task<Guid?> AddAsync(UserRolePostDto userRolePostDto, CancellationToken ct = default)
    {
        var newUserRoleId = Guid.NewGuid();

        var newUserRole = new UserRole
        {
            UserRoleId = newUserRoleId,
            UserRoleTitle = userRolePostDto.UserRoleTitle,
            EnumIndex = (int)userRolePostDto.UserRoleEnum
        };

        _appDbContext.UserRoles.Add(newUserRole);

        var checkResult = await _appDbContext.SaveChangesAsync(ct);

        if (checkResult <= 0)
            return null;

        return newUserRoleId;
    }

    public async Task<bool> UpdateAsync(Guid userRoleId, UserRolePatchDtoObsolete userRolePatchDtoObsolete, CancellationToken ct = default)
    {
        var existingUserRole = await _appDbContext.UserRoles.FirstOrDefaultAsync(ur => ur.UserRoleId == userRoleId, ct);
        
        if (existingUserRole is null)
            return false;
        
        if (userRolePatchDtoObsolete.UserRoleTitle is not null) existingUserRole.UserRoleTitle = userRolePatchDtoObsolete.UserRoleTitle;
        if (userRolePatchDtoObsolete.UserRoleEnum.HasValue) existingUserRole.EnumIndex = (int)userRolePatchDtoObsolete.UserRoleEnum.Value;
        
        var checkResult = await _appDbContext.SaveChangesAsync(ct);
        
        if (checkResult <= 0)
            return false;
        
        return true;
    }
}