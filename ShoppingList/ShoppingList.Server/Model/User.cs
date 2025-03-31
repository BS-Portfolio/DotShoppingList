namespace ShoppingList.Server.Model;

public class User
{
    public int UserID { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string EmailAddress { get; set; }
    public string Password { get; set; }
    public DateTime CreationDate { get; set; }
        
    // Navigation Properties
    public virtual ICollection<CollaboratorList> CollaboratorLists { get; set; }
}