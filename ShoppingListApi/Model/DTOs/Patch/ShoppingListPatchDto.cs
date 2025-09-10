using System.ComponentModel.DataAnnotations;

namespace ShoppingListApi.Model.DTOs.Patch;

public class ShoppingListPatchDto
{
    [Required]
    public string NewShoppingListName { get; }

    public ShoppingListPatchDto(string newShoppingListName)
    {
        NewShoppingListName = newShoppingListName;
    }
}
