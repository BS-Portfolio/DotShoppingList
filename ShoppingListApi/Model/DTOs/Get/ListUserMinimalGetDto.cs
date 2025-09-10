namespace ShoppingListApi.Model.DTOs.Get;

public class ListUserMinimalGetDto
{
    public Guid UserId { get; set; }
    public string FirstName { get; }
    public string LastName { get; }
    public string EmailAddress { get; }

    public ListUserMinimalGetDto(Guid userId, string firstName, string lastName, string emailAddress)
    {
        UserId = userId;
        FirstName = firstName;
        LastName = lastName;
        EmailAddress = emailAddress;
    }
}