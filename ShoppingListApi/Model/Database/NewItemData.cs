
using ShoppingListApi.Model.DTOs.Post;

namespace ShoppingListApi.Model.Database;

public class NewItemData
{
    public ItemPostDto ItemPostDto { get; }
    public Guid ShoppingListId { get; }
    public Guid ListOwnerId { get; }
    public Guid? RequestingUserId { get; }

    public NewItemData(ItemPostDto itemPostDto, Guid shoppingListId, Guid listOwnerId, Guid? requestingUserId = null)
    {
        this.ItemPostDto = itemPostDto;
        ShoppingListId = shoppingListId;
        ListOwnerId = listOwnerId;
        RequestingUserId = requestingUserId;
    }
}