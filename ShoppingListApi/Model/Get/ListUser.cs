using System.ComponentModel.DataAnnotations;

namespace ShoppingListApi.Model.Get;

public class ListUser
{
    public Guid UserID { get; set; }
    public string FirstName { get; }
    public string LastName { get; }
    public string EmailAddress { get; }
    public DateTimeOffset CreationDate { get; }
    public string ApiKey { get; }
    public DateTimeOffset ApiKeyExpirationDateTime { get; }

    public ListUser(Guid userId, string firstName, string lastName, string emailAddress, DateTimeOffset creationDate,
        string apiKey, DateTimeOffset apiKeyExpirationDateTime)
    {
        UserID = userId;
        FirstName = firstName;
        LastName = lastName;
        EmailAddress = emailAddress;
        CreationDate = creationDate;
        ApiKey = apiKey;
        ApiKeyExpirationDateTime = apiKeyExpirationDateTime;
    }
}