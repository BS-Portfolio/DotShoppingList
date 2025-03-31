using System.ComponentModel.DataAnnotations;

namespace ShoppingList.Server.Model.Post;

public class ItemPost
{
    [Required]
    public string ItemName { get; set; }
    [Required]
    public string ItemUnit { get; set; }
    [Required]
    public decimal ItemAmount { get; set; }      
}