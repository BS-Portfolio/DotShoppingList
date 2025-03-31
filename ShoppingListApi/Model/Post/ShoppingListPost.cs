using System.ComponentModel.DataAnnotations;

namespace ShoppingListApi.Model.Post;

public class ShoppingListPost
{
    [Required]
    public string ShoppingListName { get; set; }
}
