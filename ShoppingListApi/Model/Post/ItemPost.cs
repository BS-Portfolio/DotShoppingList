using System.ComponentModel.DataAnnotations;

namespace ShoppingListApi.Model.Post;

public class ItemPost
{
    [Required]
    public string ItemName { get; set; }
    [Required]
    public string ItemAmount { get; set; }

    public ItemPost(string itemName, string itemAmount)
    {
        ItemName = itemName;
        ItemAmount = itemAmount;
    }
}