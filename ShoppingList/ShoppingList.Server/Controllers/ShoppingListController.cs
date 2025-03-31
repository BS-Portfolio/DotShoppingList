using Microsoft.AspNetCore.Mvc;
using ShoppingList.Server.Services;

namespace ShoppingList.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class ShoppingListController : Controller
{
    
    private readonly ShoppingListService _shoppingListService;

    public ShoppingListController()
    {
        _shoppingListService = new ShoppingListService();
    }
    
    
    [HttpGet(Name = "GetShoppingListItems")]
    public ActionResult Index()
    {
        var items = _shoppingListService.GetAllItems();
        return View(items);
    }
    
}
