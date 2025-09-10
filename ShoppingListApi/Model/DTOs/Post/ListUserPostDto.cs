using System.ComponentModel.DataAnnotations;

namespace ShoppingListApi.Model.DTOs.Post;

public class ListUserPostDto
{
    [Required]
    public string FirstName { get; set; }

    [Required]
    public string LastName { get; set; }

    [Required]
    public string EmailAddress64 { get; set; }

    [Required]
    public string Password64 { get; set; }

    public ListUserPostDto(string firstName, string lastName, string emailAddress64, string password64)
    {
        FirstName = firstName;
        LastName = lastName;
        EmailAddress64 = emailAddress64;
        Password64 = password64;
    }
}