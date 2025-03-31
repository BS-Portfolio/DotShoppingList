namespace ShoppingList.Server.Model;

public class UserRole
{
    public int UserRoleID { get; set; }
    public string UserRoleTitle { get; set; }
        
    // Navigation Properties
    public virtual ICollection<CollaboratorList> CollaboratorLists { get; set; }
}