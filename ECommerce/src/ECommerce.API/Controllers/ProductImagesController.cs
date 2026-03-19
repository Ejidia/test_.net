using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ECommerce.API.Data;
using ECommerce.Core.Models;

namespace ECommerce.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductImagesController : ControllerBase
{
    private readonly ECommerceContext _context;

    public ProductImagesController(ECommerceContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductImage>>> GetProductImages()
    {
        return await _context.ProductImages.Include(pi => pi.Product).ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductImage>> GetProductImage(int id)
    {
        var productImage = await _context.ProductImages.Include(pi => pi.Product).FirstOrDefaultAsync(pi => pi.Id == id);
        return productImage == null ? NotFound() : productImage;
    }

    [HttpGet("product/{productId}")]
    public async Task<ActionResult<IEnumerable<ProductImage>>> GetProductImages(int productId)
    {
        return await _context.ProductImages.Where(pi => pi.ProductId == productId).OrderBy(pi => pi.DisplayOrder).ToListAsync();
    }

    [HttpPost]
    public async Task<ActionResult<ProductImage>> CreateProductImage(ProductImage productImage)
    {
        _context.ProductImages.Add(productImage);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetProductImage), new { id = productImage.Id }, productImage);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProductImage(int id, ProductImage productImage)
    {
        if (id != productImage.Id) return BadRequest();
        _context.Entry(productImage).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProductImage(int id)
    {
        var productImage = await _context.ProductImages.FindAsync(id);
        if (productImage == null) return NotFound();
        _context.ProductImages.Remove(productImage);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}