using System.ComponentModel.DataAnnotations;

namespace ShoppingListApi.Model.Patch;

public class ListUserPatch
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? EmailAddress { get; set; }
}