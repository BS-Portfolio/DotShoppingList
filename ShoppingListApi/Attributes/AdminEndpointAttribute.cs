namespace ShoppingListApi.Attributes;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
public class AdminEndpointAttribute : Attribute
{
}