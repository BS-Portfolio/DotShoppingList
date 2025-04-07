namespace ShoppingListApi.Model.Patch;

public class ItemPatch
{
    public string? NewItemName { get; set; }
    public string? NewItemAmount { get; set; }

    public ItemPatch(string? newItemName, string? newItemAmount)
    {
        NewItemName = newItemName;
        NewItemAmount = newItemAmount;
    }
} 