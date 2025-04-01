using System.ComponentModel.DataAnnotations;

namespace ShoppingListApi.Model.Post;

public class ItemPost
{
    [Required]
    public string ItemName { get; set; }
    [Required]
    public string ItemUnit { get; set; }
    [Required]
    public decimal ItemAmount { get; set; }

    public ItemPost(string itemName, string itemUnit, decimal itemAmount)
    {
        ItemName = itemName;
        ItemUnit = itemUnit;
        ItemAmount = itemAmount;
    }
}