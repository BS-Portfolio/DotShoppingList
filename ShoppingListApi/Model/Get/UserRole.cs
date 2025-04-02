namespace ShoppingListApi.Model.Get;

public class UserRole
{
    public Guid UserRoleId { get; set; }
    public string UserRoleTitle { get; set; }
    public int EnumIndex { get; set; }

    public UserRole(Guid userRoleId, string userRoleTitle, int enumIndex)
    {
        UserRoleId = userRoleId;
        UserRoleTitle = userRoleTitle;
        EnumIndex = enumIndex;
    }
}