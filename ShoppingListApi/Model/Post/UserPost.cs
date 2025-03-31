using System.ComponentModel.DataAnnotations;

namespace ShoppingListApi.Model.Post;

public class UserPost
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string EmailAddress { get; set; }
    public string PasswordHash { get; set; }
    public DateTimeOffset CreationDate { get; private set; }

    public UserPost()
    {    
        CreationDate = DateTimeOffset.UtcNow;
    }
}