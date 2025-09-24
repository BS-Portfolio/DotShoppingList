using ShoppingListApi.Interfaces.Repositories;

namespace ShoppingListApi.Interfaces.Services;

public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// Gets the repository for managing API keys.
    /// </summary>
    IApiKeyRepository ApiKeyRepository { get; }
    /// <summary>
    /// Gets the repository for managing email confirmation tokens.
    /// </summary>
    IEmailConfirmationTokenRepository EmailConfirmationTokenRepository { get; }
    /// <summary>
    /// Gets the repository for managing items.
    /// </summary>
    IItemRepository ItemRepository { get; }
    /// <summary>
    /// Gets the repository for managing list memberships.
    /// </summary>
    IListMembershipRepository ListMembershipRepository { get; }
    /// <summary>
    /// Gets the repository for managing list users.
    /// </summary>
    IListUserRepository ListUserRepository { get; }
    /// <summary>
    /// Gets the repository for managing shopping lists.
    /// </summary>
    IShoppingListRepository ShoppingListRepository { get; }
    /// <summary>
    /// Gets the repository for managing user roles.
    /// </summary>
    IUserRoleRepository UserRoleRepository { get; }
    
    /// <summary>
    /// Persists all changes made in the context to the database asynchronously.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken ct = default);
    /// <summary>
    /// Begins a new database transaction asynchronously.
    /// </summary>
    Task BeginTransactionAsync(CancellationToken ct = default);
    /// <summary>
    /// Commits the current database transaction asynchronously.
    /// </summary>
    Task CommitTransactionAsync(CancellationToken ct = default);
    /// <summary>
    /// Rolls back the current database transaction asynchronously.
    /// </summary>
    Task RollbackTransactionAsync(CancellationToken ct = default);

}