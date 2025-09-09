using Microsoft.EntityFrameworkCore;

namespace ShoppingListApi.Data.Contexts;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Model.Entity.ListUser> ListUsers => Set<Model.Entity.ListUser>();

    public DbSet<Model.Entity.EmailConfirmationToken> EmailConfirmationTokens =>
        Set<Model.Entity.EmailConfirmationToken>();

    public DbSet<Model.Entity.ApiKey> ApiKeys => Set<Model.Entity.ApiKey>();
    public DbSet<Model.Entity.ShoppingList> ShoppingLists => Set<Model.Entity.ShoppingList>();
    public DbSet<Model.Entity.ListMembership> ListMemberships => Set<Model.Entity.ListMembership>();
    public DbSet<Model.Entity.UserRole> UserRoles => Set<Model.Entity.UserRole>();
    public DbSet<Model.Entity.Item> Items => Set<Model.Entity.Item>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Model.Entity.ListMembership>()
            .HasKey(lm => new { lm.ShoppingListId, lm.UserId, lm.UserRoleId });
    }
}