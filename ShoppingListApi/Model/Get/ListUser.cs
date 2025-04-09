using System.ComponentModel.DataAnnotations;

namespace ShoppingListApi.Model.Get;

public class ListUser
{
    public Guid UserID { get; set; }
    public string FirstName { get; }
    public string LastName { get; }
    public string EmailAddress { get; }
    public DateTimeOffset CreationDateTime { get; }
    public string ApiKey { get; }
    public DateTimeOffset ApiKeyExpirationDateTime { get; }

    public ListUser(Guid userId, string firstName, string lastName, string emailAddress, DateTimeOffset creationDateTime,
        string apiKey, DateTimeOffset apiKeyExpirationDateTime)
    {
        UserID = userId;
        FirstName = firstName;
        LastName = lastName;
        EmailAddress = emailAddress;
        CreationDateTime = creationDateTime;
        ApiKey = apiKey;
        ApiKeyExpirationDateTime = apiKeyExpirationDateTime;
    }
}