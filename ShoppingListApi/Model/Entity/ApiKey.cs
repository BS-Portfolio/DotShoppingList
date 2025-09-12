using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography;

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
    
    public static string GenerateKey()
    {
        var randomBytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return BitConverter.ToString(randomBytes).Replace("-", "");
    }
}