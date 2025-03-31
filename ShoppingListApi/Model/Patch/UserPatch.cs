using System.ComponentModel.DataAnnotations;

namespace ShoppingListApi.Model.Patch;

public class UserPatch
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? EmailAddress { get; set; }
}