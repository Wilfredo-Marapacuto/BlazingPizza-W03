using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlazingPizza;

[ApiController]
[Route("orders")]
public class OrdersController : ControllerBase
{
    private readonly PizzaStoreContext db;

    public OrdersController(PizzaStoreContext db)
    {
        this.db = db;
    }

    [HttpGet]
    public async Task<ActionResult<List<OrderWithStatus>>> GetOrders()
    {
        var orders = await db.Orders
            .Include(o => o.Pizzas)
                .ThenInclude(p => p.Special)
            .Include(o => o.Pizzas)
                .ThenInclude(p => p.Toppings)
                    .ThenInclude(t => t.Topping)
            .OrderByDescending(o => o.CreatedTime)
            .ToListAsync();

        return orders.Select(OrderWithStatus.FromOrder).ToList();
    }

    [HttpGet("{orderId:int}")]
    public async Task<ActionResult<OrderWithStatus>> GetOrder(int orderId)
    {
        var order = await db.Orders
            .Include(o => o.Pizzas)
                .ThenInclude(p => p.Special)
            .Include(o => o.Pizzas)
                .ThenInclude(p => p.Toppings)
                    .ThenInclude(t => t.Topping)
            .SingleOrDefaultAsync(o => o.OrderId == orderId);

        if (order == null)
        {
            return NotFound();
        }

        return OrderWithStatus.FromOrder(order);
    }

    [HttpPost]
    public async Task<ActionResult<int>> PlaceOrder(Order order)
    {
        if (order.Pizzas == null || order.Pizzas.Count == 0)
        {
            return BadRequest("The order must contain at least one pizza.");
        }

        order.OrderId = 0;
        order.CreatedTime = DateTime.Now;

        foreach (var pizza in order.Pizzas)
        {
            pizza.Id = 0;
            pizza.SpecialId = pizza.Special.Id;
            pizza.Special = null;

            foreach (var topping in pizza.Toppings)
            {
                topping.Topping = null;
            }
        }

        db.Orders.Add(order);
        await db.SaveChangesAsync();

        return order.OrderId;
    }
}