using ShoppingListApi.Enums;
using ShoppingListApi.Model.Post;

namespace ShoppingListApi.Model.Database;

public class NewItemData
{
    public ItemPost ItemPost { get; }
    public Guid ShoppingListId { get; }
    public Guid ListOwnerId { get; }
    public Guid? RequestingUserId { get; }

    public NewItemData(ItemPost itemPost, Guid shoppingListId, Guid listOwnerId, Guid? requestingUserId = null)
    {
        this.ItemPost = itemPost;
        ShoppingListId = shoppingListId;
        ListOwnerId = listOwnerId;
        RequestingUserId = requestingUserId;
    }
}