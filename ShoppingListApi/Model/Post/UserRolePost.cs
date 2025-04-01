using System.ComponentModel.DataAnnotations;

namespace ShoppingListApi.Model.Post;

public class UserRolePost
{
    [Required]
    public string UserRoleTitle { get; set; }

    public UserRolePost(string userRoleTitle)
    {
        UserRoleTitle = userRoleTitle;
    }
}