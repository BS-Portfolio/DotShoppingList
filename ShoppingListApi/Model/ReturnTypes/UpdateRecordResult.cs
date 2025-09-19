namespace ShoppingListApi.Model.ReturnTypes;

public record UpdateRecordResult<TConflictingRecord>(
    bool TargetExists,
    bool Success,
    bool Conflicts,
    TConflictingRecord ConflictingRecord);
