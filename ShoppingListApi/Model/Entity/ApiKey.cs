using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShoppingListApi.Model.Entity;

public class ApiKey
{
    [Key]
    public Guid ApiKeyId { get; set; }

    [Required]
    public Guid UserId { get; set; }

    [MaxLength(200)]
    [Required]
    public required string Key { get; set; }

    [Required]
    public DateTimeOffset CreationDateTime { get; set; }

    [Required]
    public DateTimeOffset ExpirationDateTime { get; set; }

    [Required]
    public bool IsValid { get; set; }

    [ForeignKey("UserId")]
    public virtual ListUser? User { get; set; }
}