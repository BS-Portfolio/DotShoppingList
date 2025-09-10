
namespace ShoppingListApi.Model.ReturnTypes;

public class CredentialsCheckReturn
{
    public bool LoginSuccessful { get; }
    public Guid? UserId { get; }

    public CredentialsCheckReturn(bool loginSuccessful, Guid? userId = null)
    {
        LoginSuccessful = loginSuccessful;
        UserId = userId;
    }
}