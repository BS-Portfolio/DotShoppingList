using ShoppingListApi.Model.Get;

namespace ShoppingListApi.Exceptions;

public class MultipleUsersForEmailException : NumberedException
{
    public string CollidingEmailAddress { get; }
    public List<Guid> LoadedUserIds { get; }

    public MultipleUsersForEmailException(string collidingEmailAddress, List<Guid> loadedUserIds)
    {
        CollidingEmailAddress = collidingEmailAddress;
        LoadedUserIds = loadedUserIds;
    }

    public MultipleUsersForEmailException(string? message, string collidingEmailAddress, List<Guid> loadedUserIds) :
        base(message)
    {
        CollidingEmailAddress = collidingEmailAddress;
        LoadedUserIds = loadedUserIds;
    }

    public MultipleUsersForEmailException(string? message, Exception? innerException, string collidingEmailAddress,
        List<Guid> loadedUserIds) :
        base(message, innerException)
    {
        CollidingEmailAddress = collidingEmailAddress;
        LoadedUserIds = loadedUserIds;
    }
}