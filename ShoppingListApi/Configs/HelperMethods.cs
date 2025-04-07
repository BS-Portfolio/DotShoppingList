using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using ShoppingListApi.Enums;
using ShoppingListApi.Model.ReturnTypes;

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

    internal static async Task HandleAuthenticationResponse(int httpResponseCode, AuthorizationErrorEnum authEnum, HttpContext context)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = httpResponseCode;
        string jsonResponse =
            JsonConvert.SerializeObject(new AuthenticationErrorResponse(authEnum));
        byte[] responseBytes = Encoding.UTF8.GetBytes(jsonResponse);

        await context.Response.Body.WriteAsync(responseBytes);
    }
}