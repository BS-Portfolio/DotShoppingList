using System.ComponentModel.DataAnnotations;

namespace ShoppingListApi.Model.Patch;

public class ShoppingListPatch
{
    [Required]
    public string NewShoppingListName { get; }

    public ShoppingListPatch(string newShoppingListName)
    {
        NewShoppingListName = newShoppingListName;
    }
}
