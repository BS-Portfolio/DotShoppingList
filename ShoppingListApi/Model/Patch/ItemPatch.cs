namespace ShoppingListApi.Model.Patch;

public class ItemPatch
{
    public string? NewItemName { get; set; }
    public string? NewItemUnit { get; set; }
    public decimal? NewItemAmount { get; set; }

    public ItemPatch(string? newItemName, string? newItemUnit, decimal? newItemAmount)
    {
        NewItemName = newItemName;
        NewItemUnit = newItemUnit;
        NewItemAmount = newItemAmount;
    }
} 