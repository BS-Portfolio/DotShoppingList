using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography;

namespace ShoppingListApi.Model.Entity;

public class EmailConfirmationToken
{
    [Key]
    public Guid EmailConfirmationTokenId { get; set; }

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
    
    public static string GenerateToken()
    {
        const string chars = "abcdefghijklmnopqrstuvwxyz0123456789#$&*+=~";
        var tokenChars = new char[32];
        using var rng = RandomNumberGenerator.Create();
        var randomBytes = new byte[tokenChars.Length];
        rng.GetBytes(randomBytes);
        for (int i = 0; i < tokenChars.Length; i++)
        {
            tokenChars[i] = chars[randomBytes[i] % chars.Length];
        }
        return new string(tokenChars);
    }

    public static bool ValidateToken(EmailConfirmationToken token)
    {
        return token.IsUsed is false && token.ExpirationDateTime > DateTimeOffset.UtcNow;
    }
}