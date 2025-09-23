using System.ComponentModel.DataAnnotations;
using System.Text;
using ShoppingListApi.Model.DTOs.Post;

namespace ShoppingListApi.Model.DTOs.Create
;

public record ListUserCreateDto
{
    public void Deconstruct(out string firstName, out string lastName, out string emailAddress, out string passwordHash,
        out DateTimeOffset creationDateTime)
    {
        firstName = FirstName;
        lastName = LastName;
        emailAddress = EmailAddress;
        passwordHash = PasswordHash;
        creationDateTime = CreationDateTime;
    }

    [Required]
    public string FirstName { get; set; }

    [Required]
    public string LastName { get; set; }

    [Required]
    public string EmailAddress { get; set; }

    public string PasswordHash { get; private set; }
    public DateTimeOffset CreationDateTime { get; private set; }

    public ListUserCreateDto(string firstName, string lastName, string emailAddress, string password)
    {
        FirstName = firstName;
        LastName = lastName;
        EmailAddress = emailAddress;
        PasswordHash = BCrypt.Net.BCrypt.EnhancedHashPassword(password, 13);
        CreationDateTime = DateTimeOffset.UtcNow;
    }

    public ListUserCreateDto(ListUserPostDto listUserPostDto)
    {
        FirstName = listUserPostDto.FirstName;
        LastName = listUserPostDto.LastName;

        byte[] decodedEmail64StringBytes = Convert.FromBase64String(listUserPostDto.EmailAddress64);
        string decodedEmailAddress = Encoding.UTF8.GetString(decodedEmail64StringBytes);
        EmailAddress = decodedEmailAddress;

        byte[] decodedPassword64StringBytes = Convert.FromBase64String(listUserPostDto.Password64);
        string decodedPassword = Encoding.UTF8.GetString(decodedPassword64StringBytes);
        PasswordHash = BCrypt.Net.BCrypt.EnhancedHashPassword(decodedPassword, 13);

        CreationDateTime = DateTimeOffset.UtcNow;
    }
}