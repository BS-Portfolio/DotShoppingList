namespace ShoppingList.Server.Model;

public class ShoppingList
{
    public int ShoppingListID { get; set; }
    public string ShoppingListName { get; set; }
        
    // Navigation Properties
    public virtual ICollection<Item> Items { get; set; }
    public virtual ICollection<CollaboratorList> CollaboratorLists { get; set; }
}
