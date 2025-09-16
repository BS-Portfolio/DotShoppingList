using System.ComponentModel.DataAnnotations;

namespace ShoppingListApi.Model.DTOs.PatchObsolete;

public class ListUserPatchDtoObsolete
{
    public string? NewFirstName { get; set; }
    public string? NewLastName { get; set; }
    public ListUserPatchDtoObsolete(string? newFirstName, string? newLastName)
    {
        NewFirstName = newFirstName;
        NewLastName = newLastName;
    }
}