using Microsoft.EntityFrameworkCore.Storage;
using ShoppingListApi.Data.Contexts;
using ShoppingListApi.Interfaces.Repositories;
using ShoppingListApi.Interfaces.Services;
using ShoppingListApi.Repositories;

namespace ShoppingListApi.Services;

public class UnitOfWork(AppDbContext appDbContext) : IUnitOfWork
{
    private IDbContextTransaction? _transaction;

    private IApiKeyRepository? _apiKeyRepository;
    private IEmailConfirmationTokenRepository? _emailConfirmationTokenRepository;
    private IItemRepository? _itemRepository;
    private IListMembershipRepository? _listMembershipRepository;
    private IListUserRepository? _listUserRepository;
    private IShoppingListRepository? _shoppingListRepository;
    private IUserRoleRepository? _userRoleRepository;

    public IApiKeyRepository ApiKeyRepository =>
        _apiKeyRepository ??= new ApiKeyRepository(appDbContext);

    public IEmailConfirmationTokenRepository EmailConfirmationTokenRepository =>
        _emailConfirmationTokenRepository ??=
            new EmailConfirmationTokenRepository(appDbContext);

    public IItemRepository ItemRepository =>
        _itemRepository ??= new ItemRepository(appDbContext);

    public IListMembershipRepository ListMembershipRepository =>
        _listMembershipRepository ??= new ListMembershipRepository(appDbContext);

    public IListUserRepository ListUserRepository =>
        _listUserRepository ??= new ListUserRepository(appDbContext);

    public IShoppingListRepository ShoppingListRepository =>
        _shoppingListRepository ??= new ShoppingListRepository(appDbContext);

    public IUserRoleRepository UserRoleRepository =>
        _userRoleRepository ??= new UserRoleRepository(appDbContext);

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        return await appDbContext.SaveChangesAsync(ct);
    }

    public async Task BeginTransactionAsync(CancellationToken ct = default)
    {
        _transaction = await appDbContext.Database.BeginTransactionAsync(ct);
    }

    public async Task CommitTransactionAsync(CancellationToken ct = default)
    {
        if (_transaction is null)
            throw new InvalidOperationException("No transaction to commit.");

        await _transaction.CommitAsync(ct);
    }

    public async Task RollbackTransactionAsync(CancellationToken ct = default)
    {
        if (_transaction is null)
            throw new InvalidOperationException("No transaction to rollback.");

        await _transaction.RollbackAsync(ct);
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _transaction = null;
        appDbContext.Dispose();
        GC.SuppressFinalize(this);
    }
}