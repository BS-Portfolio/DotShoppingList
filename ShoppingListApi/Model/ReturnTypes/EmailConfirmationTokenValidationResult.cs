namespace ShoppingListApi.Model.ReturnTypes;

public record EmailConfirmationTokenValidationResult(bool TokenExists, bool TokenIsValid);