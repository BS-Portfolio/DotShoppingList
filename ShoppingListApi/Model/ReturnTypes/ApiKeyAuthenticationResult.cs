namespace ShoppingListApi.Model.ReturnTypes;

public record ApiKeyAuthenticationResult(
    bool AccountExists,
    bool IsAuthenticated,
    bool? ApiKeyExists,
    bool? ApiKeyIsValid,
    bool? ApiKeyIsExpired
);