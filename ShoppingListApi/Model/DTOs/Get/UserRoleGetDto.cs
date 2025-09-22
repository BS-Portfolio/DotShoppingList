using ShoppingListApi.Model.Entity;

namespace ShoppingListApi.Model.DTOs.Get;

public record UserRoleGetDto(Guid UserRoleId, string UserRoleTitle)
{
    public static UserRoleGetDto FromUserRole(UserRole userRole)
    {
        return new UserRoleGetDto(userRole.UserRoleId, userRole.UserRoleTitle);
    }

    public static List<UserRoleGetDto> FromUserRoleList(List<UserRole> userRoles)
    {
        return userRoles.Select(FromUserRole).ToList();
    }
}