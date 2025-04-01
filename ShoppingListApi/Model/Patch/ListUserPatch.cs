using System.ComponentModel.DataAnnotations;

namespace ShoppingListApi.Model.Patch;

public class ListUserPatch
{
    public string? NewFirstName { get; set; }
    public string? NewLastName { get; set; }
    public string? NewEmailAddress { get; set; }

    public ListUserPatch(string? newFirstName, string? newLastName, string? newEmailAddress)
    {
        NewFirstName = newFirstName;
        NewLastName = newLastName;
        NewEmailAddress = newEmailAddress;
    }
}