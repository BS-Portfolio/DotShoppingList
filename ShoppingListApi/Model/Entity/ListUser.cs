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
    public Guid PasswordHash { get; set; }

    public virtual List<ApiKey> ApiKeys { get; set; } = [];
    public virtual List<EmailConfirmationToken> EmailConfirmationTokens { get; set; } = [];
    public virtual List<ListMembership> ListMemberships { get; set; } = [];
}