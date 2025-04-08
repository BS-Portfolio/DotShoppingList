namespace ShoppingListApi.Model.ReturnTypes;

public class RecordFetchResult<T>
{
    public T Record { get; }
    public bool? RecordExists { get; }
    public bool? AccessGranted { get; }

    public RecordFetchResult(T record, bool? accessGranted = null, bool? recordExists = null)
    {
        AccessGranted = accessGranted;
        RecordExists = recordExists;
        Record = record;
    }
}