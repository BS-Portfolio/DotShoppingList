using ShoppingListApi.Model.Entity;

namespace ShoppingListApi.Model.DTOs.Get;

public record ListUserGetDto
{
    public ListUserGetDto(Guid userId, string firstName, string lastName, string emailAddress,
        DateTimeOffset creationDateTime, string apiKey, DateTimeOffset apiKeyExpirationDateTime,
        DateTimeOffset? expirationDateTime = null)
    {
        UserId = userId;
        FirstName = firstName;
        LastName = lastName;
        EmailAddress = emailAddress;
        CreationDateTime = creationDateTime;
        ApiKey = apiKey;
        ApiKeyExpirationDateTime = apiKeyExpirationDateTime;
        ExpirationDateTime = expirationDateTime;
    }

    public ListUserGetDto(ListUser user, ApiKey apiKey)
    {
        UserId = user.UserId;
        FirstName = user.FirstName;
        LastName = user.LastName;
        EmailAddress = user.EmailAddress;
        CreationDateTime = user.CreationDateTime;
        ExpirationDateTime = user.ExpirationDateTime;
        ApiKey = apiKey.Key;
        ApiKeyExpirationDateTime = apiKey.ExpirationDateTime;
    }

    public Guid UserId { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string EmailAddress { get; set; }
    public DateTimeOffset CreationDateTime { get; set; }
    public string ApiKey { get; set; }
    public DateTimeOffset ApiKeyExpirationDateTime { get; set; }
    public DateTimeOffset? ExpirationDateTime { get; set; } = null;
}