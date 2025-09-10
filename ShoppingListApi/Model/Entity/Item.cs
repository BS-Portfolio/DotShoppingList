using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShoppingListApi.Model.Entity;

public class Item
{
    [Key]
    public Guid ItemId { get; set; }

    [Required]
    public Guid ShoppingListId { get; set; }

    [Required]
    [MaxLength(200)]
    public required string ItemName { get; set; }

    [Required]
    [MaxLength(200)]
    public required string ItemAmount { get; set; }

    [ForeignKey("ShoppingListId")]
    public virtual ShoppingList? ShoppingList { get; set; }
}