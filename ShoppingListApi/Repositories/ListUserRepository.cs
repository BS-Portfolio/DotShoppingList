using Microsoft.EntityFrameworkCore;
using ShoppingListApi.Data.Contexts;
using ShoppingListApi.Interfaces.Repositories;
using ShoppingListApi.Model.DTOs.Create;
using ShoppingListApi.Model.DTOs.Patch;
using ShoppingListApi.Model.Entity;

namespace ShoppingListApi.Repositories;

public class ListUserRepository(AppDbContext appAppDbContext) : IListUserRepository
{
    private readonly AppDbContext _appDbContext = appAppDbContext;


    /// <summary>
    /// Retrieves a ListUser by its ID without including related entities.
    /// Returns null if not found.
    /// </summary>
    public async Task<ListUser?> GetWithoutDetailsByIdAsync(Guid listUserId, CancellationToken ct = default)
    {
        return await _appDbContext.ListUsers.FindAsync([listUserId], ct);
    }

    /// <summary>
    /// Retrieves a ListUser by its email address without including related entities.
    /// Returns null if not found.
    /// </summary>
    public async Task<ListUser?> GetWithoutDetailsByEmailAddressAsync(string emailAddress,
        CancellationToken ct = default)
    {
        return await _appDbContext.ListUsers.FirstOrDefaultAsync(lu => lu.EmailAddress == emailAddress, ct);
    }

    /// <summary>
    /// Retrieves a ListUser by its ID, including related ApiKeys, EmailConfirmationTokens, ListMemberships, ShoppingLists, and UserRoles.
    /// Returns null if not found.
    /// </summary>
    public async Task<ListUser?> GetWithDetailsByIdAsync(Guid listUserId, CancellationToken ct = default)
    {
        return await _appDbContext.ListUsers
            .Include(u => u.ApiKeys)
            .Include(u => u.EmailConfirmationTokens)
            .Include(u => u.ListMemberships).ThenInclude(lm => lm.ShoppingList)
            .Include(u => u.ListMemberships).ThenInclude(lm => lm.UserRole)
            .FirstOrDefaultAsync(lu => lu.UserId == listUserId, ct);
    }

    /// <summary>
    /// Retrieves a ListUser by its email address, including related ApiKeys, EmailConfirmationTokens, ListMemberships, ShoppingLists, and UserRoles.
    /// Returns null if not found.
    /// </summary>
    public async Task<ListUser?> GetWithDetailsByEmailAddressAsync(string emailAddress,
        CancellationToken ct = default)
    {
        return await _appDbContext.ListUsers
            .Include(u => u.ApiKeys)
            .Include(u => u.EmailConfirmationTokens)
            .Include(u => u.ListMemberships).ThenInclude(lm => lm.ShoppingList)
            .Include(u => u.ListMemberships).ThenInclude(lm => lm.UserRole)
            .FirstOrDefaultAsync(lu => lu.EmailAddress == emailAddress, ct);
    }
    
    /// <summary>
    /// Retrieves all ListUsers without including related entities.
    /// </summary>
    public async Task<List<ListUser>> GetAllWithoutDetailsAsync(CancellationToken ct = default)
    {
        return await _appDbContext.ListUsers.ToListAsync(ct);
    }

    /// <summary>
    /// Creates a new ListUser from the provided DTO and returns its ID.
    /// Does not save changes to the database.
    /// </summary>
    public async Task<Guid> CreateAsync(ListUserCreateDto listUserCreateDto, CancellationToken ct = default)
    {
        var newUserId = Guid.NewGuid();

        var newUser = new ListUser()
        {
            UserId = newUserId,
            FirstName = listUserCreateDto.FirstName,
            LastName = listUserCreateDto.LastName,
            EmailAddress = listUserCreateDto.EmailAddress,
            PasswordHash = listUserCreateDto.PasswordHash,
            CreationDateTime = listUserCreateDto.CreationDateTime
        };

        await _appDbContext.ListUsers.AddAsync(newUser, ct);

        return newUserId;
    }

    /// <summary>
    /// Updates the first and/or last name of the specified ListUser using the provided patch DTO.
    /// </summary>
    public void UpdateName(ListUser listUser, ListUserPatchDto listUserPatchDto)
    {
        if (listUserPatchDto.FirstName is null && listUserPatchDto.LastName is null)
            return;

        if (listUserPatchDto.FirstName is not null)
            listUser.FirstName = listUserPatchDto.FirstName;

        if (listUserPatchDto.LastName is not null)
            listUser.LastName = listUserPatchDto.LastName;
    }

    /// <summary>
    /// Updates the password hash of the specified ListUser.
    /// </summary>
    public void UpdatePassword(ListUser listUser, string newPasswordHash)
    {
        listUser.PasswordHash = newPasswordHash;
    }

    /// <summary>
    /// Sets the expiration date/time for the specified ListUser.
    /// </summary>
    public void SetExpirationDateTime(ListUser listUser, DateTimeOffset? expirationDateTime)
    {
        listUser.ExpirationDateTime = expirationDateTime;
    }

    /// <summary>
    /// Removes the specified ListUser from the database context. Does not save changes.
    /// </summary>
    public void Delete(ListUser listUser)
    {
        _appDbContext.ListUsers.Remove(listUser);
    }
}