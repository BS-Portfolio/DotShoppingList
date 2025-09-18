using System.ComponentModel.DataAnnotations;
using ShoppingListApi.Enums;

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

    public virtual ICollection<ListMembership> ListMemberships { get; set; } = [];
    
    public static UserRoleEnum? GetEnumFromIndex(int index)
    {
        var isDefined = Enum.IsDefined(typeof(UserRoleEnum), index);
        
        if (isDefined)
            return (UserRoleEnum)index;

        return null;
    }
}