using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShoppingListApi.Model.Entity;

public class EmailConfirmationToken
{
    public Guid EmailConfirmationTokeId { get; set; }

    public Guid UserId { get; set; }

    [MaxLength(200)]
    [Required]
    public required string Token { get; set; }

    [Required]
    public DateTimeOffset CreationDateTime { get; set; }

    [Required]
    public DateTimeOffset ExpirationDateTime { get; set; }

    [Required]
    public bool IsUsed { get; set; }

    [ForeignKey("UserId")]
    public virtual ListUser? User { get; set; }
}