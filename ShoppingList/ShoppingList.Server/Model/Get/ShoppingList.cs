namespace ShoppingList.Server.Model.Get;

public class ShoppingList
{
    public Guid ShoppingListID { get; set; }
    public string ShoppingListName { get; set; }
        
    // Navigation Properties
    public virtual ICollection<Item> Items { get; set; }
    public virtual ICollection<ListMember> Members { get; set; }
}
