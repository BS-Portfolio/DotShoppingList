using System.ComponentModel.DataAnnotations;

namespace ShoppingListApi.Model.Entity;

public class ShoppingList()
{
    public Guid ShoppingListId { get; set; }

    [MaxLength(50)]
    [Required]
    public required string ShoppingListName { get; set; }
    
    public virtual List<ListMembership> ListMemberships { get; set; } = [];
}