using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ECommerce.API.Data;
using ECommerce.Core.Models;

namespace ECommerce.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AddressesController : ControllerBase
{
    private readonly ECommerceContext _context;

    public AddressesController(ECommerceContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Address>>> GetAddresses()
    {
        return await _context.Addresses.Include(a => a.User).ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Address>> GetAddress(int id)
    {
        var address = await _context.Addresses.Include(a => a.User).FirstOrDefaultAsync(a => a.Id == id);
        return address == null ? NotFound() : address;
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<IEnumerable<Address>>> GetUserAddresses(int userId)
    {
        return await _context.Addresses.Where(a => a.UserId == userId).ToListAsync();
    }

    [HttpPost]
    public async Task<ActionResult<Address>> CreateAddress(Address address)
    {
        _context.Addresses.Add(address);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetAddress), new { id = address.Id }, address);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAddress(int id, Address address)
    {
        if (id != address.Id) return BadRequest();
        _context.Entry(address).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAddress(int id)
    {
        var address = await _context.Addresses.FindAsync(id);
        if (address == null) return NotFound();
        _context.Addresses.Remove(address);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}