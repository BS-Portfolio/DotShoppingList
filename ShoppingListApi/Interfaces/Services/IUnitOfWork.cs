using ShoppingListApi.Interfaces.Repositories;

namespace ShoppingListApi.Interfaces.Services;

public interface IUnitOfWork : IDisposable
{
    IApiKeyRepository ApiKeyRepository { get; }
    IEmailConfirmationTokenRepository EmailConfirmationTokenRepository { get; }
    IItemRepository ItemRepository { get; }
    IListMembershipRepository ListMembershipRepository { get; }
    IListUserRepository ListUserRepository { get; }
    IShoppingListRepository ShoppingListRepository { get; }
    IUserRoleRepository UserRoleRepository { get; }
    
    Task<int> SaveChangesAsync(CancellationToken ct = default);
    Task BeginTransactionAsync(CancellationToken ct = default);
    Task CommitTransactionAsync(CancellationToken ct = default);
    Task RollbackTransactionAsync(CancellationToken ct = default);
    

}