namespace ShoppingListApi.Model.Delete;

public class ListUserDelete
{
    public string EmailAddress { get; }

    public ListUserDelete(string emailAddress)
    {
        EmailAddress = emailAddress;
    }
}