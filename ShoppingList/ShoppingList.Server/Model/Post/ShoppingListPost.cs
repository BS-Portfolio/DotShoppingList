using System.ComponentModel.DataAnnotations;

namespace ShoppingList.Server.Model.Post;

public class ShoppingListPost
{
    [Required]
    public string ShoppingListName { get; set; }
}
