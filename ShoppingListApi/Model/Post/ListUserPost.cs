using System.ComponentModel.DataAnnotations;

namespace ShoppingListApi.Model.Post;

public class ListUserPost
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string EmailAddress { get; set; }
    public string PasswordHash { get; set; }
    public DateTimeOffset CreationDate { get; private set; }

    public ListUserPost()
    {    
        CreationDate = DateTimeOffset.UtcNow;
    }
}