using System.ComponentModel.DataAnnotations;

namespace ShoppingListApi.Model.Post;

public class UserRolePost
{
    [Required] public string UserRoleTitle { get; }
    public P.UserRoleEnum UserRoleEnum { get; }

    public UserRolePost(string userRoleTitle, P.UserRoleEnum userRoleEnum)
    {
        UserRoleTitle = userRoleTitle;
        UserRoleEnum = userRoleEnum;
    }
}