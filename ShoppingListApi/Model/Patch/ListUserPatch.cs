using System.ComponentModel.DataAnnotations;

namespace ShoppingListApi.Model.Patch;

public class ListUserPatch
{
    public string? NewFirstName { get; set; }
    public string? NewLastName { get; set; }
    public ListUserPatch(string? newFirstName, string? newLastName)
    {
        NewFirstName = newFirstName;
        NewLastName = newLastName;
    }
}