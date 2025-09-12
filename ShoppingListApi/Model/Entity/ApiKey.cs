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
        const string chars = "abcdefghijklmnopqrstuvwxyz0123456789#$&*+=~";
        var tokenChars = new char[64];
        
        using var rng = RandomNumberGenerator.Create();
        var randomBytes = new byte[tokenChars.Length];
        rng.GetBytes(randomBytes);
        
        for (int i = 0; i < tokenChars.Length; i++)
        {
            tokenChars[i] = chars[randomBytes[i] % chars.Length];
        }
        
        return new string(tokenChars);
    }
    
    public static bool ValidateKey(ApiKey apiKey)
    {
        return apiKey.IsValid && apiKey.ExpirationDateTime > DateTimeOffset.UtcNow;
    }
}