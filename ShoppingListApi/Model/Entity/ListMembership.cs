using System.ComponentModel.DataAnnotations.Schema;

namespace ShoppingListApi.Model.Entity;

public class ListMembership
{
    public Guid ShoppingListId { get; set; }
    public Guid UserId { get; set; }
    public Guid UserRoleId { get; set; }

    [ForeignKey("ShoppingListId")]
    public virtual ShoppingList? ShoppingList { get; set; }

    [ForeignKey("UserId")]
    public virtual ListUser? User { get; set; }

    [ForeignKey("UserRoleId")]
    public virtual UserRole? UserRole { get; set; }
}