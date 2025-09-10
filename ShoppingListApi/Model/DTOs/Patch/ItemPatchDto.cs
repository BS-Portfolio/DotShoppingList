namespace ShoppingListApi.Model.DTOs.Patch;

public class ItemPatchDto
{
    public string? NewItemName { get; set; }
    public string? NewItemAmount { get; set; }

    public ItemPatchDto(string? newItemName, string? newItemAmount)
    {
        NewItemName = newItemName;
        NewItemAmount = newItemAmount;
    }
} 