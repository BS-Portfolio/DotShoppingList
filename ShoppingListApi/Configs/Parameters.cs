namespace ShoppingListApi.Configs;

public static class Parameters
{
    // Test Parameter to show how to declare it
    public static string ConnectionString { get; } = "Data Source=shoppinglist.db";

    public enum UserRoleEnum
    {
        ListAdmin = 1,
        Collaborator = 2
    };
    
}