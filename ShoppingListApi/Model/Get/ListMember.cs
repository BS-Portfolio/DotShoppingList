namespace ShoppingListApi.Model.Get;

public class ListMember
{
    public ListUser ListUser { get; set; }
    public UserRole UserRole { get; set; }

    public ListMember(ListUser listUser, UserRole userRole)
    {
        ListUser = listUser;
        UserRole = userRole;
    }
}