namespace ShoppingList.Server.Model.Get;

public class Item
{
    public Guid ItemID { get; set; }
    public string ItemName { get; set; }
    public string ItemUnit { get; set; }
    public decimal ItemAmount { get; set; }      
}