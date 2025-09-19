namespace ShoppingListApi.Model.ReturnTypes;

public record RemoveRestrictedRecordResult(
    bool? TargetExists,
    bool? AccessGranted,
    bool Success,
    int RecordsAffected
);