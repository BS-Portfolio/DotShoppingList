using Microsoft.Data.SqlClient;
using ShoppingList.Server.Model;

namespace ShoppingList.Server.Services;

public class ShoppingListService
{
    
    // ToDo: Ab in die Parameters damit!!!
    private readonly string _connectionString = "your_connection_string_here";

    public List<ShoppingListItem> GetAllItems()
    {
        var items = new List<ShoppingListItem>();

        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            var command = new SqlCommand("SELECT Id, Name, Quantity, Price FROM ShoppingList", connection);
            var reader = command.ExecuteReader();

            while (reader.Read())
            {
                items.Add(new ShoppingListItem
                {
                    Id = (int)reader["Id"],
                    Name = reader["Name"].ToString(),
                    Quantity = (int)reader["Quantity"],
                    Price = (decimal)reader["Price"]
                });
            }
        }

        return items;
    }

}