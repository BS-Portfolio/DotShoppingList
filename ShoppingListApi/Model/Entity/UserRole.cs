using System.ComponentModel.DataAnnotations;

namespace ShoppingListApi.Model.Entity;

public class UserRole
{
    [Key]
    public Guid UserRoleId { get; set; }

    [MaxLength(50)]
    [Required]
    public required string UserRoleTitle { get; set; }

    [Range(0, 10)]
    [Required]
    public int EnumIndex { get; set; }

    public virtual List<ListMembership> ListMemberships { get; set; } = [];
}