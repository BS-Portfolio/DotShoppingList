using ShoppingListApi.Model.Entity;

namespace ShoppingListApi.Model.DTOs.Get;

public record ListUserMinimalGetDto
{
    public Guid UserId { get; set; }
    public string FirstName { get; }
    public string LastName { get; }
    public string EmailAddress { get; }

    public ListUserMinimalGetDto(Guid userId, string firstName, string lastName, string emailAddress)
    {
        UserId = userId;
        FirstName = firstName;
        LastName = lastName;
        EmailAddress = emailAddress;
    }

    public ListUserMinimalGetDto(ListUser listUser)
    {
        UserId = listUser.UserId;
        FirstName = listUser.FirstName;
        LastName = listUser.LastName;
        EmailAddress = listUser.EmailAddress;
    }

    public static List<ListUserMinimalGetDto> FromListUserBatch(List<ListUser> listUsers)
    {
        return listUsers.Select(listUser => new ListUserMinimalGetDto(listUser)).ToList();
    }
}