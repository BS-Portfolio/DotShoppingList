using ShoppingListApi.Enums;

namespace ShoppingListApi.Model.DTOs.Patch;

public record UserRolePatchDto(
    string? UserRoleTitle,
    UserRoleEnum? UserRoleEnum
    );