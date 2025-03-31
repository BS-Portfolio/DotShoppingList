namespace ShoppingList.Server.Model;

public class Item
{
    public int ItemID { get; set; }
    public int ShoppingListID { get; set; }
    public string ItemName { get; set; }
    public string ItemUnit { get; set; }
    public decimal ItemAmount { get; set; }
        
    // Navigation Properties
    public virtual ShoppingList ShoppingList { get; set; }
}