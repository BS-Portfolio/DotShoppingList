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

    /// <summary>
    /// Gets the repository for managing API keys.
    /// </summary>
    public IApiKeyRepository ApiKeyRepository =>
        _apiKeyRepository ??= new ApiKeyRepository(appDbContext);

    /// <summary>
    /// Gets the repository for managing email confirmation tokens.
    /// </summary>
    public IEmailConfirmationTokenRepository EmailConfirmationTokenRepository =>
        _emailConfirmationTokenRepository ??=
            new EmailConfirmationTokenRepository(appDbContext);

    /// <summary>
    /// Gets the repository for managing items.
    /// </summary>
    public IItemRepository ItemRepository =>
        _itemRepository ??= new ItemRepository(appDbContext);

    /// <summary>
    /// Gets the repository for managing list memberships.
    /// </summary>
    public IListMembershipRepository ListMembershipRepository =>
        _listMembershipRepository ??= new ListMembershipRepository(appDbContext);

    /// <summary>
    /// Gets the repository for managing list users.
    /// </summary>
    public IListUserRepository ListUserRepository =>
        _listUserRepository ??= new ListUserRepository(appDbContext);

    /// <summary>
    /// Gets the repository for managing shopping lists.
    /// </summary>
    public IShoppingListRepository ShoppingListRepository =>
        _shoppingListRepository ??= new ShoppingListRepository(appDbContext);

    /// <summary>
    /// Gets the repository for managing user roles.
    /// </summary>
    public IUserRoleRepository UserRoleRepository =>
        _userRoleRepository ??= new UserRoleRepository(appDbContext);

    /// <summary>
    /// Persists all changes made in the context to the database asynchronously.
    /// </summary>
    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        return await appDbContext.SaveChangesAsync(ct);
    }

    /// <summary>
    /// Begins a new database transaction asynchronously.
    /// </summary>
    public async Task BeginTransactionAsync(CancellationToken ct = default)
    {
        _transaction = await appDbContext.Database.BeginTransactionAsync(ct);
    }

    /// <summary>
    /// Commits the current database transaction asynchronously.
    /// Throws InvalidOperationException if no transaction exists.
    /// </summary>
    public async Task CommitTransactionAsync(CancellationToken ct = default)
    {
        if (_transaction is null)
            throw new InvalidOperationException("No transaction to commit.");

        await _transaction.CommitAsync(ct);
    }

    /// <summary>
    /// Rolls back the current database transaction asynchronously.
    /// Throws InvalidOperationException if no transaction exists.
    /// </summary>
    public async Task RollbackTransactionAsync(CancellationToken ct = default)
    {
        if (_transaction is null)
            throw new InvalidOperationException("No transaction to rollback.");

        await _transaction.RollbackAsync(ct);
    }

    /// <summary>
    /// Disposes the transaction and the database context.
    /// </summary>
    public void Dispose()
    {
        _transaction?.Dispose();
        _transaction = null;
        appDbContext.Dispose();
        GC.SuppressFinalize(this);
    }
}