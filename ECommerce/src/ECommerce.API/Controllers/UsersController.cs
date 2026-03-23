using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ECommerce.API.Data;
using ECommerce.Core.Models;

namespace ECommerce.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly ECommerceContext _context;

    public UsersController(ECommerceContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<User>>> GetUsers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] string? role = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] string sortBy = "name",
        [FromQuery] bool sortDesc = false)
    {
        var query = _context.Users.AsQueryable();

        // Search filter
        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(u => u.FirstName.Contains(search) || u.LastName.Contains(search) || u.Email.Contains(search));
        }

        // Role filter
        if (!string.IsNullOrEmpty(role) && Enum.TryParse<ECommerce.Core.Enums.UserRole>(role, true, out var userRole))
        {
            query = query.Where(u => u.Role == userRole);
        }

        // Active status filter
        if (isActive.HasValue)
        {
            query = query.Where(u => u.IsActive == isActive.Value);
        }

        // Sorting
        query = sortBy.ToLower() switch
        {
            "email" => sortDesc ? query.OrderByDescending(u => u.Email) : query.OrderBy(u => u.Email),
            "date" => sortDesc ? query.OrderByDescending(u => u.CreatedAt) : query.OrderBy(u => u.CreatedAt),
            "role" => sortDesc ? query.OrderByDescending(u => u.Role) : query.OrderBy(u => u.Role),
            _ => sortDesc ? query.OrderByDescending(u => u.FirstName) : query.OrderBy(u => u.FirstName)
        };

        // Pagination
        var totalItems = await query.CountAsync();
        var users = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        Response.Headers.Add("X-Total-Count", totalItems.ToString());
        Response.Headers.Add("X-Page", page.ToString());
        Response.Headers.Add("X-Page-Size", pageSize.ToString());

        return users;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<User>> GetUser(
        int id, 
        [FromQuery] bool includeOrders = false,
        [FromQuery] bool includeReviews = false)
    {
        var query = _context.Users.AsQueryable();
        
        if (includeOrders)
        {
            query = query.Include(u => u.Orders);
        }
        
        if (includeReviews)
        {
            query = query.Include(u => u.Reviews).ThenInclude(r => r.Product);
        }

        var user = await query.FirstOrDefaultAsync(u => u.Id == id);
        return user == null ? NotFound() : user;
    }

    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<User>>> SearchUsers(
        [FromQuery] string email,
        [FromQuery] int limit = 10)
    {
        if (string.IsNullOrEmpty(email))
            return BadRequest("Email parameter is required");

        var users = await _context.Users
            .Where(u => u.Email.Contains(email))
            .Take(limit)
            .ToListAsync();

        return users;
    }

    [HttpPost]
    public async Task<ActionResult<User>> CreateUser(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(int id, User user)
    {
        if (id != user.Id) return BadRequest();
        _context.Entry(user).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return NotFound();
        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}