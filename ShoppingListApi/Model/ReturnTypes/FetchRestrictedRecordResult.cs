namespace ShoppingListApi.Model.ReturnTypes;

public record FetchRestrictedRecordResult<T>(T Record, bool? AccessGranted = null, bool? RecordExists = null);