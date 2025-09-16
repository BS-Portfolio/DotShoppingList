namespace ShoppingListApi.Model.ReturnTypes;

public record AddItemResult(
    bool ShoppingListExists,
    bool? AccessGranted,
    bool Success,
    bool? MaxAmountReached,
    Guid? ItemId);