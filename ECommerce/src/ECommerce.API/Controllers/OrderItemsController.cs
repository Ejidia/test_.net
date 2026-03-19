using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ECommerce.API.Data;
using ECommerce.Core.Models;

namespace ECommerce.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrderItemsController : ControllerBase
{
    private readonly ECommerceContext _context;

    public OrderItemsController(ECommerceContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<OrderItem>>> GetOrderItems()
    {
        return await _context.OrderItems.Include(oi => oi.Order).Include(oi => oi.Product).ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<OrderItem>> GetOrderItem(int id)
    {
        var orderItem = await _context.OrderItems.Include(oi => oi.Order).Include(oi => oi.Product).FirstOrDefaultAsync(oi => oi.Id == id);
        return orderItem == null ? NotFound() : orderItem;
    }

    [HttpGet("order/{orderId}")]
    public async Task<ActionResult<IEnumerable<OrderItem>>> GetOrderItems(int orderId)
    {
        return await _context.OrderItems.Where(oi => oi.OrderId == orderId).Include(oi => oi.Product).ToListAsync();
    }

    [HttpPost]
    public async Task<ActionResult<OrderItem>> CreateOrderItem(OrderItem orderItem)
    {
        _context.OrderItems.Add(orderItem);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetOrderItem), new { id = orderItem.Id }, orderItem);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateOrderItem(int id, OrderItem orderItem)
    {
        if (id != orderItem.Id) return BadRequest();
        _context.Entry(orderItem).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteOrderItem(int id)
    {
        var orderItem = await _context.OrderItems.FindAsync(id);
        if (orderItem == null) return NotFound();
        _context.OrderItems.Remove(orderItem);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}