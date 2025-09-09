using System.ComponentModel.DataAnnotations;

namespace ShoppingListApi.Model.Entity;

public class UserRole
{
    public Guid UserRoleId { get; set; }

    [MaxLength(50)]
    public required string UserRoleTitle { get; set; }

    [Range(0, 10)]
    public int EnumIndex { get; set; }
    
    public virtual List<ListMembership> ListMemberships { get; set; } = [];
}