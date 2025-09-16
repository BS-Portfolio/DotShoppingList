namespace ShoppingListApi.Model.ReturnTypes;

public record AddRecordResult<TAddedRecord, TConflictingRecord>(
    bool Success,
    TAddedRecord? AddedRecord,
    bool Conflicts,
    TConflictingRecord? ConflictingRecord
    );