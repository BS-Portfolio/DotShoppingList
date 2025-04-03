using ShoppingListApi.Model.Post;

namespace ShoppingListApi.Model.Database;

public class NewItemData
{
    public ItemPost itemPost { get; }
    public Guid ShoppingListId { get; }

    public NewItemData(ItemPost itemPost, Guid shoppingListId)
    {
        this.itemPost = itemPost;
        ShoppingListId = shoppingListId;
    }
}