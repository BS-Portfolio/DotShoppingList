namespace ShoppingListApi.Model.DTOs.Patch;

public record ListUserPatchDto(
    string? FirstName,
    string? LastName
    );