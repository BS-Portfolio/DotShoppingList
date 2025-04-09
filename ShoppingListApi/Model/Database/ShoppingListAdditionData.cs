using System.ComponentModel.DataAnnotations;

namespace ShoppingListApi.Model.Database;

public class ShoppingListAdditionData
{
    public string ShoppingListName { get; set; }
    public Guid UserId { get; set; }

    public ShoppingListAdditionData(string shoppingListName, Guid userId)
    {
        ShoppingListName = shoppingListName;
        UserId = userId;
    }
}