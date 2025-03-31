namespace ShoppingList.Server.Model.Post;

public class ShoppingList
{
    public int ShoppingListID { get; set; }
    public string ShoppingListName { get; set; }
        
    // Navigation Properties
    public virtual ICollection<ItemPost> Items { get; set; }
    public virtual ICollection<ListMemberPost> Members { get; set; }
}
