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
        
        // ApiKey
        modelBuilder.Entity<Model.Entity.ApiKey>()
            .HasIndex(x => x.UserId);
        modelBuilder.Entity<Model.Entity.ApiKey>()
            .HasIndex(x => x.Key);
        modelBuilder.Entity<Model.Entity.ApiKey>()
            .HasIndex(x => x.ExpirationDateTime);
        modelBuilder.Entity<Model.Entity.ApiKey>()
            .HasIndex(x => x.IsValid);

        // EmailConfirmationToke
        modelBuilder.Entity<Model.Entity.EmailConfirmationToken>()
            .HasIndex(x => x.UserId);
        modelBuilder.Entity<Model.Entity.EmailConfirmationToken>()
            .HasIndex(x => x.Token);
        modelBuilder.Entity<Model.Entity.EmailConfirmationToken>()
            .HasIndex(x => x.ExpirationDateTime);
        modelBuilder.Entity<Model.Entity.EmailConfirmationToken>()
            .HasIndex(x => x.IsUsed);

        // Item
        modelBuilder.Entity<Model.Entity.Item>()
            .HasIndex(x => x.ShoppingListId);

        // ListUser
        modelBuilder.Entity<Model.Entity.ListUser>()
            .HasIndex(x => x.EmailAddress);
        
        // UserRole
        modelBuilder.Entity<Model.Entity.UserRole>()
            .HasIndex(x => x.EnumIndex);
    }
}