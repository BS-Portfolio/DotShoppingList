using System.ComponentModel.DataAnnotations;

namespace ShoppingListApi.Model.Patch;

public class UserRolePatch
{
    [Required]
    public string NewUserRoleTitle { get; set; }

    public UserRolePatch(string newUserRoleTitle)
    {
        NewUserRoleTitle = newUserRoleTitle;
    }
}