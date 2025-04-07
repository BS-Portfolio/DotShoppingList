using ShoppingListApi.Enums;
using ShoppingListApi.Model.Post;

namespace ShoppingListApi.Model.Database;

public class NewItemData
{
    public ItemPost ItemPost { get; }
    public Guid ShoppingListId { get; }
    
    public Guid UserId { get; }

    public NewItemData(ItemPost itemPost, Guid shoppingListId, Guid userId)
    {
        this.ItemPost = itemPost;
        ShoppingListId = shoppingListId;
        UserId = userId;
    }
}