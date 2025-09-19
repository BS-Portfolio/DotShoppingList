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


    public async Task<ListUser?> GetWithoutDetailsByIdAsync(Guid listUserId, CancellationToken ct = default)
    {
        return await _appDbContext.ListUsers.FindAsync([listUserId], ct);
    }

    public async Task<ListUser?> GetWithoutDetailsByEmailAddressAsync(string emailAddress,
        CancellationToken ct = default)
    {
        return await _appDbContext.ListUsers.FirstOrDefaultAsync(lu => lu.EmailAddress == emailAddress, ct);
    }

    public async Task<Guid> CreateAsync(ListUserCreateDto listUserCreateDto, CancellationToken ct= default)
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

    public void UpdateName(ListUser listUser, ListUserPatchDto listUserPatchDto)
    {
        if (listUserPatchDto.FirstName is null && listUserPatchDto.LastName is null)
            return;

        if (listUserPatchDto.FirstName is not null)
            listUser.FirstName = listUserPatchDto.FirstName;

        if (listUserPatchDto.LastName is not null)
            listUser.LastName = listUserPatchDto.LastName;
    }

    public void UpdatePassword(ListUser listUser, string newPasswordHash)
    {
        listUser.PasswordHash = newPasswordHash;
    }

    public void Delete(ListUser listUser)
    {
        _appDbContext.ListUsers.Remove(listUser);
    }

}