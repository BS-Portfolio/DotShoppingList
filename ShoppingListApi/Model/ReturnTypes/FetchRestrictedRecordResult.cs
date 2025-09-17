namespace ShoppingListApi.Model.ReturnTypes;

public class FetchRestrictedRecordResult<T>
{
    public T Record { get; }
    public bool? RecordExists { get; }
    public bool? AccessGranted { get; }

    public FetchRestrictedRecordResult(T record, bool? accessGranted = null, bool? recordExists = null)
    {
        AccessGranted = accessGranted;
        RecordExists = recordExists;
        Record = record;
    }
}