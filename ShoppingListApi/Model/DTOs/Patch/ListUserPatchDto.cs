using System.ComponentModel.DataAnnotations;

namespace ShoppingListApi.Model.DTOs.Patch;

public class ListUserPatchDto
{
    public string? NewFirstName { get; set; }
    public string? NewLastName { get; set; }
    public ListUserPatchDto(string? newFirstName, string? newLastName)
    {
        NewFirstName = newFirstName;
        NewLastName = newLastName;
    }
}