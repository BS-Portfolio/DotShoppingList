using System.Security.Cryptography;

namespace ShoppingListApi.Configs;

public class HelperMethods
{
    internal static string GenerateApiKey()
    {
        var randomBytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return BitConverter.ToString(randomBytes).Replace("-", "");
    }
}