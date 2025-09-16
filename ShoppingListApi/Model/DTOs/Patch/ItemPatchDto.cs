namespace ShoppingListApi.Model.DTOs.Patch;

public record ItemPatchDto(
    string? ItemName,
    string? ItemAmount
    );
