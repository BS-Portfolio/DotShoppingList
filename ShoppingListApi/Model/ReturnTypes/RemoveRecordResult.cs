namespace ShoppingListApi.Model.ReturnTypes;

public record RemoveRecordResult(
    bool TargetExists,
    bool Success,
    int RecordsAffected
    );