using System.ComponentModel.DataAnnotations;

namespace ShoppingListApi.Model.Get;

public class ListUser
{
    public Guid UserID { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string EmailAddress { get; set; }
    public DateTime CreationDate { get; set; }

    public ListUser(Guid userId, string firstName, string lastName, string emailAddress, DateTime creationDate)
    {
        UserID = userId;
        FirstName = firstName;
        LastName = lastName;
        EmailAddress = emailAddress;
        CreationDate = creationDate;
    }
}