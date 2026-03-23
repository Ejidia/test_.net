using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ECommerce.API.Data;
using ECommerce.Core.Models;

namespace ECommerce.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly ECommerceContext _context;

    public ProductsController(ECommerceContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Product>>> GetProducts(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] int? categoryId = null,
        [FromQuery] decimal? minPrice = null,
        [FromQuery] decimal? maxPrice = null,
        [FromQuery] string sortBy = "name",
        [FromQuery] bool sortDesc = false)
    {
        var query = _context.Products.Include(p => p.Category).AsQueryable();

        // Search filter
        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(p => p.Name.Contains(search) || p.Description.Contains(search));
        }

        // Category filter
        if (categoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == categoryId.Value);
        }

        // Price filters
        if (minPrice.HasValue)
        {
            query = query.Where(p => p.Price >= minPrice.Value);
        }
        if (maxPrice.HasValue)
        {
            query = query.Where(p => p.Price <= maxPrice.Value);
        }

        // Sorting
        query = sortBy.ToLower() switch
        {
            "price" => sortDesc ? query.OrderByDescending(p => p.Price) : query.OrderBy(p => p.Price),
            "date" => sortDesc ? query.OrderByDescending(p => p.CreatedAt) : query.OrderBy(p => p.CreatedAt),
            _ => sortDesc ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name)
        };

        // Pagination
        var totalItems = await query.CountAsync();
        var products = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        Response.Headers.Add("X-Total-Count", totalItems.ToString());
        Response.Headers.Add("X-Page", page.ToString());
        Response.Headers.Add("X-Page-Size", pageSize.ToString());

        return products;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Product>> GetProduct(int id, [FromQuery] bool includeReviews = false)
    {
        var query = _context.Products.Include(p => p.Category).AsQueryable();
        
        if (includeReviews)
        {
            query = query.Include(p => p.Reviews).ThenInclude(r => r.User);
        }

        var product = await query.FirstOrDefaultAsync(p => p.Id == id);
        return product == null ? NotFound() : product;
    }

    [HttpPost]
    public async Task<ActionResult<Product>> CreateProduct(Product product)
    {
        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(int id, Product product)
    {
        if (id != product.Id) return BadRequest();
        _context.Entry(product).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null) return NotFound();
        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}