using ShoppingListApi.Model.Entity;

namespace ShoppingListApi.Model.DTOs.Get;

public record ItemGetDto(Guid ItemId, string ItemName, string ItemAmount)
{
    public Guid ItemId { get; set; } = ItemId;
    public string ItemName { get; set; } = ItemName;
    public string ItemAmount { get; set; } = ItemAmount;

    public ItemGetDto(Item item) : this(item.ItemId, item.ItemName, item.ItemAmount)
    {
    }

    public static List<ItemGetDto> FromItemBatch(List<Item> items)
    {
        return items.Select(item => new ItemGetDto(item)).ToList();
    }
}