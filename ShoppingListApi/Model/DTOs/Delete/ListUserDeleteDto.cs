namespace ShoppingListApi.Model.DTOs.Delete;

public class ListUserDeleteDto
{
    public string EmailAddress { get; }

    public ListUserDeleteDto(string emailAddress)
    {
        EmailAddress = emailAddress;
    }
}