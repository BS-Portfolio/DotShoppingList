namespace ShoppingListApi.Model.ReturnTypes;

public record AppAuthenticationResult(
    bool AccountExists,
    bool IsAuthenticated,
    bool? ApiKeyExists,
    bool? ApiKeyIsValid,
    bool? ApiKeyIsExpired
);