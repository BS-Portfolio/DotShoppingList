using System.ComponentModel.DataAnnotations;

namespace ShoppingListApi.Model.DTOs.PatchObsolete;

public class ShoppingListPatchDtoObsolete
{
    [Required]
    public string NewShoppingListName { get; }

    public ShoppingListPatchDtoObsolete(string newShoppingListName)
    {
        NewShoppingListName = newShoppingListName;
    }
}
