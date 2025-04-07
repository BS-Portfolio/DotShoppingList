using System.ComponentModel.DataAnnotations;

namespace ShoppingListApi.Model.Database;

public class ShoppingListAdditionData
{
    [Required] public string ShoppingListName { get; set; }

    [Required] public Guid UserId { get; set; }

    public ShoppingListAdditionData(string shoppingListName, Guid userId)
    {
        ShoppingListName = shoppingListName;
        UserId = userId;
    }
}