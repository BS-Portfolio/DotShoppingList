namespace ShoppingListApi.Model.ReturnTypes;

public record ResponseResult<T>(T Data, string Message);