using System;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.IO;
using Microsoft.Data.SqlClient;
using Xunit;

namespace ShoppingListApi.Tests;

public class UnitTests
{
    [Fact]
    public void Check_Database_Connection()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();
            
        // Du kannst hier wählen, welche Verbindung getestet werden soll
        var connectionString = configuration.GetConnectionString("Milad");
        var canConnect = false;
        
        // Act
        try
        {
            using var connection = new SqlConnection(connectionString);
            connection.Open();
            canConnect = connection.State == System.Data.ConnectionState.Open;
            connection.Close();
        }
        catch (Exception ex)
        {
            // Fehler bei der Verbindung - Test wird fehlschlagen
            Console.WriteLine($"Verbindungsfehler: {ex.Message}");
        }
        
        // Assert
        Assert.True(canConnect, "Die Verbindung zur Datenbank konnte nicht hergestellt werden");
    }
}