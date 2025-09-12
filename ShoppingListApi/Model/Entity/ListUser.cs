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

    public virtual ICollection<ApiKey> ApiKeys { get; set; } = new List<ApiKey>();
    public virtual ICollection<EmailConfirmationToken> EmailConfirmationTokens { get; set; } = new List<EmailConfirmationToken>();
    public virtual ICollection<ListMembership> ListMemberships { get; set; } = new List<ListMembership>();
}