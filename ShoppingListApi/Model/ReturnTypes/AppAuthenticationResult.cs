namespace ShoppingListApi.Model.ReturnTypes;

public record AppAuthenticationResult(
    bool AccountExists,
    bool IsAuthenticated,
    bool ApiKeyWasEqual,
    bool ApiKeyIsValid,
    bool? ProgramFailure
);