using ShoppingListApi.Model.DTOs.Get;
using ShoppingListApi.Configs;
using ShoppingListApi.Model.Entity;

namespace ShoppingListApi.Test;

public class UnitTests
{
    public UnitTests()
    {
        
    }
    
    [Fact]
    public void ShoppingList_AddItemsToShoppingList_AddsItems()
    {
        // Arrange
        var listId = Guid.NewGuid();
        var owner = new ListUserMinimalGetDto(Guid.NewGuid(), "John", "Doe", "john@example.com");
        var shoppingList = new ShoppingListGetDto(listId, "Grocery List", owner);
        
        var items = new List<ItemGetDto>
        {
            new ItemGetDto(Guid.NewGuid(), "Milk", "2 liters"),
            new ItemGetDto(Guid.NewGuid(), "Bread", "1 loaf")
        };

        // Act
        shoppingList.AddItemsToShoppingList(items);

        // Assert
        Assert.Equal(2, shoppingList.Items.Count);
        Assert.Contains(items[0], shoppingList.Items);
        Assert.Contains(items[1], shoppingList.Items);
    }


    [Fact]
    public void GenerateApiKey_ReturnsNonEmptyString()
    {
        // Act
        var apiKey = ApiKey.GenerateKey();

        // Assert
        Assert.NotNull(apiKey);
        Assert.NotEmpty(apiKey);
        Assert.True(apiKey.Length > 10);
    }

    [Fact]
    public void GenerateApiKey_ReturnsDifferentKeysOnMultipleCalls()
    {
        // Act
        var key1 = ApiKey.GenerateKey();
        var key2 = ApiKey.GenerateKey();
        var key3 = ApiKey.GenerateKey();

        // Assert
        Assert.NotEqual(key1, key2);
        Assert.NotEqual(key1, key3);
        Assert.NotEqual(key2, key3);
    }

    [Fact]
    public void UserRoleEnum_HasExpectedValues()
    {
        // Assert
        Assert.Equal(1, (int)Enums.UserRoleEnum.ListOwner);
        Assert.Equal(2, (int)Enums.UserRoleEnum.Collaborator);
    }

}