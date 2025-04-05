namespace ShoppingListApi.Model.Get;

public class ListUserMinimal
{
    public Guid UserId { get; set; }
    public string FirstName { get; }
    public string LastName { get; }
    public string EmailAddress { get; }

    public ListUserMinimal(Guid userId, string firstName, string lastName, string emailAddress)
    {
        UserId = userId;
        FirstName = firstName;
        LastName = lastName;
        EmailAddress = emailAddress;
    }
}