using ShoppingListApi.Enums;
using ShoppingListApi.Model.DTOs.Patch;
using ShoppingListApi.Model.DTOs.Post;
using ShoppingListApi.Model.Entity;

namespace ShoppingListApi.Interfaces.Repositories;

public interface IUserRoleRepository
{
    /// <summary>
    /// Retrieves all UserRole entities from the database.
    /// </summary>
    Task<List<UserRole>> GetAllAsync(CancellationToken ct = default);

    /// <summary>
    /// Retrieves a UserRole by its unique ID.
    /// Returns null if not found.
    /// </summary>
    Task<UserRole?> GetByIdAsync(Guid userRoleId, CancellationToken ct = default);

    /// <summary>
    /// Retrieves a UserRole by its enum value.
    /// Returns null if not found.
    /// </summary>
    Task<UserRole?> GetByEnumAsync(UserRoleEnum userRoleEnum, CancellationToken ct = default);

    /// <summary>
    /// Retrieves a UserRole by its title string.
    /// Returns null if not found.
    /// </summary>
    Task<UserRole?> GetByTitleAsync(string userRoleTitle, CancellationToken ct = default);

    /// <summary>
    /// Adds a new UserRole to the database using the provided DTO and returns its ID.
    /// Does not save changes to the database.
    /// </summary>
    Task<Guid> AddAsync(UserRolePostDto userRolePostDto, CancellationToken ct = default);

    /// <summary>
    /// Updates the title and/or enum value of the specified UserRole using the provided patch DTO.
    /// </summary>
    void Update(UserRole targetUserRole, UserRolePatchDto userRolePatchDto);
}