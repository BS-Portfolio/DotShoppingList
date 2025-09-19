namespace ShoppingListApi.Model.ReturnTypes;

public record AddItemResult(
    bool Success,
    bool? ShoppingListExists,
    bool? AccessGranted,
    bool? MaxAmountReached,
    Guid? ItemId);