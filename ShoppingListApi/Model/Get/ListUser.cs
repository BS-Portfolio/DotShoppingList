using System.ComponentModel.DataAnnotations;

namespace ShoppingListApi.Model.Get;

public class ListUser
{
    public Guid UserID { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string EmailAddress { get; set; }
    public string PasswordHash { get; set; }
    public DateTime CreationDate { get; set; }
}