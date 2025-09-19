using ShoppingListApi.Enums;

namespace ShoppingListApi.Model.DTOs.PatchObsolete;

public record UserRolePatchDtoObsolete(
    string? UserRoleTitle,
    UserRoleEnum? UserRoleEnum
    );