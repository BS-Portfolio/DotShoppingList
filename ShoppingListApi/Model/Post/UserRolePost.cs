using System.ComponentModel.DataAnnotations;
using ShoppingListApi.Enums;

namespace ShoppingListApi.Model.Post;

public class UserRolePost
{
    [Required] public string UserRoleTitle { get; }
    public UserRoleEnum UserRoleEnum { get; }

    public UserRolePost(string userRoleTitle, UserRoleEnum userRoleEnum)
    {
        UserRoleTitle = userRoleTitle;
        UserRoleEnum = userRoleEnum;
    }
}