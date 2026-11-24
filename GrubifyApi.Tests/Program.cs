using GrubifyApi.Controllers;
using GrubifyApi.Models;
using Microsoft.AspNetCore.Mvc;

var controller = new CartController();
var userId = $"cart-memory-test-{Guid.NewGuid():N}";
var request = new AddCartItemRequest { FoodItemId = 1, Quantity = 1 };

GC.Collect();
GC.WaitForPendingFinalizers();
var before = GC.GetTotalMemory(forceFullCollection: true);

for (var i = 0; i < 12; i++)
{
    var response = controller.AddItemToCart(userId, request);
    if (response.Result is not OkObjectResult { Value: Cart cart } ||
        cart.Items.Count != 1 || cart.Items[0].Quantity != i + 1)
    {
        throw new Exception($"Cart add failed on request {i + 1}.");
    }
}

var retainedBytes = GC.GetTotalMemory(forceFullCollection: true) - before;
if (retainedBytes > 30 * 1024 * 1024)
{
    throw new Exception($"Repeated cart adds retained {retainedBytes:N0} bytes of managed memory.");
}

Console.WriteLine($"PASS: 12 cart adds retained {retainedBytes:N0} bytes; item quantity is 12.");
