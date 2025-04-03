namespace ShoppingListApi.Model.Get;

public class Item
{
    public Guid ItemID { get; set; }
    public string ItemName { get; set; }
    public string ItemAmount { get; set; }

    public Item(Guid itemId, string itemName, string itemAmount)
    {
        ItemID = itemId;
        ItemName = itemName;
        ItemAmount = itemAmount;
    }
}