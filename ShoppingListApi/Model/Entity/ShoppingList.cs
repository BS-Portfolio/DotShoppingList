using System.ComponentModel.DataAnnotations;

namespace ShoppingListApi.Model.Entity;

public class ShoppingList()
{
    [Key]
    public Guid ShoppingListId { get; set; }

    [MaxLength(50)]
    [Required]
    public required string ShoppingListName { get; set; }
    
    public virtual ICollection<ListMembership> ListMemberships { get; set; } = [];
}