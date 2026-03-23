using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ECommerce.API.Data;
using ECommerce.Core.Models;

namespace ECommerce.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly ECommerceContext _context;

    public OrdersController(ECommerceContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Order>>> GetOrders(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] int? userId = null,
        [FromQuery] string? status = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] string sortBy = "date",
        [FromQuery] bool sortDesc = true)
    {
        var query = _context.Orders.Include(o => o.OrderItems).ThenInclude(oi => oi.Product).AsQueryable();

        // User filter
        if (userId.HasValue)
        {
            query = query.Where(o => o.UserId == userId.Value);
        }

        // Status filter
        if (!string.IsNullOrEmpty(status) && Enum.TryParse<ECommerce.Core.Enums.OrderStatus>(status, true, out var orderStatus))
        {
            query = query.Where(o => o.Status == orderStatus);
        }

        // Date range filter
        if (startDate.HasValue)
        {
            query = query.Where(o => o.OrderDate >= startDate.Value);
        }
        if (endDate.HasValue)
        {
            query = query.Where(o => o.OrderDate <= endDate.Value);
        }

        // Sorting
        query = sortBy.ToLower() switch
        {
            "total" => sortDesc ? query.OrderByDescending(o => o.Total) : query.OrderBy(o => o.Total),
            "status" => sortDesc ? query.OrderByDescending(o => o.Status) : query.OrderBy(o => o.Status),
            _ => sortDesc ? query.OrderByDescending(o => o.OrderDate) : query.OrderBy(o => o.OrderDate)
        };

        // Pagination
        var totalItems = await query.CountAsync();
        var orders = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        Response.Headers.Add("X-Total-Count", totalItems.ToString());
        Response.Headers.Add("X-Page", page.ToString());
        Response.Headers.Add("X-Page-Size", pageSize.ToString());

        return orders;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Order>> GetOrder(int id, [FromQuery] bool includeItems = true)
    {
        var query = _context.Orders.AsQueryable();
        
        if (includeItems)
        {
            query = query.Include(o => o.OrderItems).ThenInclude(oi => oi.Product);
        }

        var order = await query.FirstOrDefaultAsync(o => o.Id == id);
        return order == null ? NotFound() : order;
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<IEnumerable<Order>>> GetUserOrders(
        int userId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? status = null)
    {
        var query = _context.Orders.Where(o => o.UserId == userId).Include(o => o.OrderItems).AsQueryable();

        if (!string.IsNullOrEmpty(status) && Enum.TryParse<ECommerce.Core.Enums.OrderStatus>(status, true, out var orderStatus))
        {
            query = query.Where(o => o.Status == orderStatus);
        }

        var totalItems = await query.CountAsync();
        var orders = await query
            .OrderByDescending(o => o.OrderDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        Response.Headers.Add("X-Total-Count", totalItems.ToString());
        return orders;
    }

    [HttpPost]
    public async Task<ActionResult<Order>> CreateOrder(Order order)
    {
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateOrder(int id, Order order)
    {
        if (id != order.Id) return BadRequest();
        _context.Entry(order).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteOrder(int id)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order == null) return NotFound();
        _context.Orders.Remove(order);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}