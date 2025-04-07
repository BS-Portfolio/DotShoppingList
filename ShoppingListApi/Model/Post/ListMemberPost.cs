namespace ShoppingListApi.Model.Post;

public class ListMemberPost
{
    public string UserEmailAddress { get; set; }
    public Guid UserRoleId { get; set; }

    public ListMemberPost(string userEmailAddress, Guid userRoleId)
    {
        UserEmailAddress = userEmailAddress;
        UserRoleId = userRoleId;
    }
}