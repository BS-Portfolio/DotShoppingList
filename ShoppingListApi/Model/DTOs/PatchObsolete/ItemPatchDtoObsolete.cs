namespace ShoppingListApi.Model.DTOs.PatchObsolete;

public class ItemPatchDtoObsolete
{
    public string? NewItemName { get; set; }
    public string? NewItemAmount { get; set; }

    public ItemPatchDtoObsolete(string? newItemName, string? newItemAmount)
    {
        NewItemName = newItemName;
        NewItemAmount = newItemAmount;
    }
} 