using Microsoft.EntityFrameworkCore;
using ShoppingListApi.Model.Entity;

namespace ShoppingListApi.Data.Contexts;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<ListUser> ListUsers => Set<ListUser>();

    public DbSet<EmailConfirmationToken> EmailConfirmationTokens =>
        Set<EmailConfirmationToken>();

    public DbSet<ApiKey> ApiKeys => Set<ApiKey>();
    public DbSet<ShoppingList> ShoppingLists => Set<ShoppingList>();
    public DbSet<ListMembership> ListMemberships => Set<ListMembership>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<Item> Items => Set<Item>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ListMembership>()
            .HasKey(lm => new { lm.ShoppingListId, lm.UserId, lm.UserRoleId });

        // ApiKey
        modelBuilder.Entity<ApiKey>()
            .HasIndex(x => x.UserId);
        modelBuilder.Entity<ApiKey>()
            .HasIndex(x => x.Key)
            .IsUnique();
        modelBuilder.Entity<ApiKey>()
            .HasIndex(x => x.ExpirationDateTime);
        modelBuilder.Entity<ApiKey>()
            .HasIndex(x => x.IsValid);

        // EmailConfirmationToken
        modelBuilder.Entity<EmailConfirmationToken>()
            .HasIndex(x => x.UserId);
        modelBuilder.Entity<EmailConfirmationToken>()
            .HasIndex(x => x.Token)
            .IsUnique();
        modelBuilder.Entity<EmailConfirmationToken>()
            .HasIndex(x => x.ExpirationDateTime);
        modelBuilder.Entity<EmailConfirmationToken>()
            .HasIndex(x => x.IsUsed)
            .IsUnique()
            .HasFilter("[IsUsed] = 0");

        // Item
        modelBuilder.Entity<Item>()
            .HasIndex(x => x.ShoppingListId);

        // ListUser
        modelBuilder.Entity<ListUser>()
            .HasIndex(x => x.EmailAddress);

        // UserRole
        modelBuilder.Entity<UserRole>()
            .HasIndex(x => x.EnumIndex)
            .IsUnique();
        modelBuilder.Entity<UserRole>()
            .HasIndex(x => x.UserRoleTitle)
            .IsUnique();
    }
}