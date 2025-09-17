using Microsoft.EntityFrameworkCore.Storage;
using ShoppingListApi.Data.Contexts;
using ShoppingListApi.Interfaces.Repositories;
using ShoppingListApi.Interfaces.Services;
using ShoppingListApi.Repositories;

namespace ShoppingListApi.Services;

public class UnitOfWork : IUnitOfWork
{
    private IDbContextTransaction? _transaction;
    private readonly AppDbContext _appDbContext;
    private readonly ILogger<EmailConfirmationTokenRepository> _emailConfirmationTokenRepositoryLogger;
    private readonly ILogger<ItemRepository> _itemRepositoryLogger;
    private readonly ILogger<ListMembershipRepository> _listMembershipRepositoryLogger;
    private readonly ILogger<ListUserRepository> _listUserRepositoryLogger;
    private readonly ILogger<ShoppingListRepository> _shoppingListRepositoryLogger;
    private readonly ILogger<UserRoleRepository> _userRoleRepositorLogger;

    private IApiKeyRepository? _apiKeyRepository;
    private IEmailConfirmationTokenRepository? _emailConfirmationTokenRepository;
    private IItemRepository? _itemRepository;
    private IListMembershipRepository? _listMembershipRepository;
    private IListUserRepository? _listUserRepository;
    private IShoppingListRepository? _shoppingListRepository;
    private IUserRoleRepository? _userRoleRepository;

    public UnitOfWork(
        AppDbContext appDbContext,
        ILogger<ItemRepository> itemRepositoryLogger,
        ILogger<ListMembershipRepository> listMembershipRepositoryLogger,
        ILogger<ListUserRepository> listUserRepositoryLogger,
        ILogger<ShoppingListRepository> shoppingListRepositoryLogger,
        ILogger<UserRoleRepository> userRoleRepositorLogger)
    {
        _appDbContext = appDbContext;
        _itemRepositoryLogger = itemRepositoryLogger;
        _listMembershipRepositoryLogger = listMembershipRepositoryLogger;
        _listUserRepositoryLogger = listUserRepositoryLogger;
        _shoppingListRepositoryLogger = shoppingListRepositoryLogger;
        _userRoleRepositorLogger = userRoleRepositorLogger;
    }

    public IApiKeyRepository ApiKeyRepository =>
        _apiKeyRepository ??= new ApiKeyRepository(_appDbContext);

    public IEmailConfirmationTokenRepository EmailConfirmationTokenRepository =>
        _emailConfirmationTokenRepository ??=
            new EmailConfirmationTokenRepository(_appDbContext);

    public IItemRepository ItemRepository =>
        _itemRepository ??= new ItemRepository(_appDbContext, _itemRepositoryLogger);

    public IListMembershipRepository ListMembershipRepository =>
        _listMembershipRepository ??= new ListMembershipRepository(_appDbContext, _listMembershipRepositoryLogger);

    public IListUserRepository ListUserRepository =>
        _listUserRepository ??= new ListUserRepository(_appDbContext, _listUserRepositoryLogger);

    public IShoppingListRepository ShoppingListRepository =>
        _shoppingListRepository ??= new ShoppingListRepository(_appDbContext, _shoppingListRepositoryLogger);

    public IUserRoleRepository UserRoleRepository =>
        _userRoleRepository ??= new UserRoleRepository(_appDbContext, _userRoleRepositorLogger);

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        return await _appDbContext.SaveChangesAsync(ct);
    }

    public async Task BeginTransactionAsync(CancellationToken ct = default)
    {
        _transaction = await _appDbContext.Database.BeginTransactionAsync(ct);
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
        _appDbContext.Dispose();
        GC.SuppressFinalize(this);
    }
}