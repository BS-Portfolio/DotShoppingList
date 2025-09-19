using ShoppingListApi.Model.Entity;

namespace ShoppingListApi.Model.DTOs.Get;

public class UserRoleGetDto
{
    public Guid UserRoleId { get; set; }
    public string UserRoleTitle { get; set; }
    public int EnumIndex { get; set; }

    public UserRoleGetDto(Guid userRoleId, string userRoleTitle, int enumIndex)
    {
        UserRoleId = userRoleId;
        UserRoleTitle = userRoleTitle;
        EnumIndex = enumIndex;
    }
    
    public static UserRoleGetDto FromUserRole(UserRole userRole)
    {
        return new UserRoleGetDto(userRole.UserRoleId, userRole.UserRoleTitle, userRole.EnumIndex);
    }
    
    public static List<UserRoleGetDto> FromUserRoleList(List<UserRole> userRoles)
    {
        return userRoles.Select(FromUserRole).ToList();
    }
}