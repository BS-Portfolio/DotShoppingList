namespace ShoppingListApi.Model.ReturnTypes;

public record LogoutResult(bool Success, bool ApiKeyFound, bool? UserOwnsApiKey, bool? InvalidationSuccessful);