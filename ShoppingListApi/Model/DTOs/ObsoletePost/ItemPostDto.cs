using System.ComponentModel.DataAnnotations;

namespace ShoppingListApi.Model.DTOs.ObsoletePost;

public class ItemPostDto
{
    [Required]
    public string ItemName { get; set; }
    [Required]
    public string ItemAmount { get; set; }

    public ItemPostDto(string itemName, string itemAmount)
    {
        ItemName = itemName;
        ItemAmount = itemAmount;
    }
}