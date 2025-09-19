using System.ComponentModel.DataAnnotations;
using ShoppingListApi.Enums;

namespace ShoppingListApi.Model.DTOs.ObsoletePost;

public class UserRolePostDto
{
    [Required] public string UserRoleTitle { get; }
    [Required] public UserRoleEnum UserRoleEnum { get; }

    public UserRolePostDto(string userRoleTitle, UserRoleEnum userRoleEnum)
    {
        UserRoleTitle = userRoleTitle;
        UserRoleEnum = userRoleEnum;
    }
}