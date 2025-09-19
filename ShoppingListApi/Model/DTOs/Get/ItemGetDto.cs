using ShoppingListApi.Model.Entity;

namespace ShoppingListApi.Model.DTOs.Get;

public record ItemGetDto
{
    public Guid ItemId { get; set; }
    public string ItemName { get; set; }
    public string ItemAmount { get; set; }

    public ItemGetDto(Guid itemId, string itemName, string itemAmount)
    {
        ItemId = itemId;
        ItemName = itemName;
        ItemAmount = itemAmount;
    }

    public ItemGetDto(Item item)
    {
        ItemId = item.ItemId;
        ItemName = item.ItemName;
        ItemAmount = item.ItemAmount;
    }

    public static List<ItemGetDto> FromItemBatch(List<Item> items)
    {
        return items.Select(item => new ItemGetDto(item)).ToList();
    }
}