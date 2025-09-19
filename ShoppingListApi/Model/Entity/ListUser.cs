using System.ComponentModel.DataAnnotations;

namespace ShoppingListApi.Model.Entity;

public class ListUser
{
    [Key]
    public Guid UserId { get; set; }

    [Required]
    [MaxLength(200)]
    public required string FirstName { get; set; }

    [Required]
    [MaxLength(200)]
    public required string LastName { get; set; }

    [Required]
    [MaxLength(200)]
    public required string EmailAddress { get; set; }

    [Required]
    [MaxLength(500)]
    public required string PasswordHash { get; set; }

    public DateTimeOffset CreationDateTime { get; set; }

    public virtual ICollection<ApiKey> ApiKeys { get; set; } = [];

    public virtual ICollection<EmailConfirmationToken> EmailConfirmationTokens { get; set; } =
        [];

    public virtual ICollection<ListMembership> ListMemberships { get; set; } = [];
}