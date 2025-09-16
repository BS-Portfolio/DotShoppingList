namespace ShoppingListApi.Model.ReturnTypes;

public record UpdateRestrictedRecordResult<TConflictingRecord>(
    bool? TargetExists,
    bool Success,
    bool? AccessGranted,
    bool? Conflicts,
    TConflictingRecord ConflictingRecord);