using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using ShoppingListApi.Enums;
using ShoppingListApi.Model.ReturnTypes;

namespace ShoppingListApi.Configs;

public class HelperMethods
{
    public static string GenerateApiKey()
    {
        var randomBytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return BitConverter.ToString(randomBytes).Replace("-", "");
    }

}