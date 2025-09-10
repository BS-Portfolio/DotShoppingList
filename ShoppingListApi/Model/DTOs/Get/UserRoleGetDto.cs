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
}