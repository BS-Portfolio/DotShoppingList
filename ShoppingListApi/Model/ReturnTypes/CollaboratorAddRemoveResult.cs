namespace ShoppingListApi.Model.ReturnTypes;

public class CollaboratorAddRemoveResult
{
    public bool Success { get; }
    public bool? ListOwnerIdIsValid { get; }
    public bool? CollaboratorIdIsValid { get; }
    public bool? RequestingPartyIdIsValid { get; }

    public CollaboratorAddRemoveResult(bool success, bool? listOwnerIdIsValid = null, bool? collaboratorIdIsValid = null, bool? requestingPartyIdIsValid = null)
    {
        Success = success;
        ListOwnerIdIsValid = listOwnerIdIsValid;
        CollaboratorIdIsValid = collaboratorIdIsValid;
        RequestingPartyIdIsValid = requestingPartyIdIsValid;
    }
}