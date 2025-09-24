using ShoppingListApi.Model.DTOs.Get;

namespace ShoppingListApi.Model.ReturnTypes;

public record LoginResult(bool LoginSuccessful, bool AccountExists, bool? PasswordIsValid, bool? ApiKeyGenerationSuccessful ,ListUserGetDto? TargetUser);