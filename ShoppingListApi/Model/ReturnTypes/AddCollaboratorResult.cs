namespace ShoppingListApi.Model.ReturnTypes;

public record AddCollaboratorResult(
    bool Success,
    bool? ShoppingListExists = null,
    bool? RequestingUserIsListOwner = null,
    bool? CollaboratorIsRegistered = null,
    bool? CollaboratorIsAlreadyAdded = null,
    bool? UserRoleIdNotFound = null
);