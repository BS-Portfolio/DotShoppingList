using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShoppingListApi.Model.Entity;

public class ListMembership
{
    [Required]
    public Guid ShoppingListId { get; set; }

    [Required]
    public Guid UserId { get; set; }

    [Required]
    public Guid UserRoleId { get; set; }

    [ForeignKey("ShoppingListId")]
    public virtual ShoppingList? ShoppingList { get; set; }

    [ForeignKey("UserId")]
    public virtual ListUser? User { get; set; }

    [ForeignKey("UserRoleId")]
    public virtual UserRole? UserRole { get; set; }
}