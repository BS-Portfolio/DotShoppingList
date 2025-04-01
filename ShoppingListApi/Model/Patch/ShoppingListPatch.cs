using System.ComponentModel.DataAnnotations;

namespace ShoppingListApi.Model.Patch;

public class ShoppingListPatch
{
    [Required]
    public string NewShoppingListName { get; set; }

    public ShoppingListPatch(string newShoppingListName)
    {
        NewShoppingListName = newShoppingListName;
    }
}
