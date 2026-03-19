namespace ECommerce.Core.Models;

public class CartItem
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public User User { get; set; } = null!;
    public Product Product { get; set; } = null!;

    // Computed properties
    public decimal SubTotal => Product.DiscountPrice ?? Product.Price * Quantity;
}
