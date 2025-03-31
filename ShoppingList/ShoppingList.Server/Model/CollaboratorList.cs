namespace ShoppingList.Server.Model;

public class CollaboratorList
{
    public int CollaboratorListID { get; set; }
    public int ShoppingListID { get; set; }
    public int UserID { get; set; }
    public int UserRoleID { get; set; }
        
    // Navigation Properties
    public virtual ShoppingList ShoppingList { get; set; }
    public virtual User User { get; set; }
    public virtual UserRole UserRole { get; set; }
}