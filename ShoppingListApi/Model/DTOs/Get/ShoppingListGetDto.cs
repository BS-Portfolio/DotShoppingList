namespace ShoppingListApi.Model.DTOs.Get;

public class ShoppingListGetDto
{
    public Guid ShoppingListId { get; private set; }
    public string ShoppingListName { get; private set; }
    public ListUserMinimalGetDto ListOwner { get; private set; }
    public List<ItemGetDto> Items { get; private set; }
    public List<ListUserMinimalGetDto> Collaborators { get; private set; }
    
    public ShoppingListGetDto(Guid shoppingListId, string shoppingListName, ListUserMinimalGetDto listOwner)
    {
        ShoppingListId = shoppingListId;
        ShoppingListName = shoppingListName;
        ListOwner = listOwner;
        Items = [];
        Collaborators = [];
    }

    public void AddItemsToShoppingList(List<ItemGetDto> items)
    {
        Items = items;
    }

    public void AddCollaboratorsToShoppingList(List<ListUserMinimalGetDto> collaborators)
    {
        Collaborators = collaborators;
    }
}