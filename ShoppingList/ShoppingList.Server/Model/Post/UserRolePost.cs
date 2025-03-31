using System.ComponentModel.DataAnnotations;

namespace ShoppingList.Server.Model.Post;

public class UserRolePost
{
    [Required]
    public string UserRoleTitle { get; set; }
}