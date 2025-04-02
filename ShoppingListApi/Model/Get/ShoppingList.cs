namespace ShoppingListApi.Model.Get;

public class ShoppingList
{
    public Guid ShoppingListId { get; private set; }
    public string ShoppingListName { get; private set; }
    public ListUser ListOwner { get; private set; }
    public List<Item> Items { get; private set; }
    public List<ListUser> Collaborators { get; private set; }
    
    public ShoppingList(Guid shoppingListId, string shoppingListName, ListUser listOwner)
    {
        ShoppingListId = shoppingListId;
        ShoppingListName = shoppingListName;
        ListOwner = listOwner;
        Items = [];
        Collaborators = [];
    }

    public void AddItemsToShoppingList(List<Item> items)
    {
        Items = items;
    }

    public void AddCollaboratorsToShoppingList(List<ListUser> collaborators)
    {
        Collaborators = collaborators;
    }
}