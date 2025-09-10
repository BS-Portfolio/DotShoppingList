namespace ShoppingListApi.Model.DTOs.Get;

public class ItemGetDto
{
    public Guid ItemID { get; set; }
    public string ItemName { get; set; }
    public string ItemAmount { get; set; }

    public ItemGetDto(Guid itemId, string itemName, string itemAmount)
    {
        ItemID = itemId;
        ItemName = itemName;
        ItemAmount = itemAmount;
    }
}