namespace ShoppingListApi.Model.ReturnTypes;

public class UpdateResult
{
    public bool Success { get; }
    public bool AccessGranted { get; }

    public UpdateResult(bool success, bool accessGranted)
    {
        Success = success;
        AccessGranted = accessGranted;
    }
}