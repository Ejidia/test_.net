using Microsoft.EntityFrameworkCore;
using ECommerce.Core.Models;

namespace ECommerce.API.Data;

public class ECommerceContext : DbContext
{
    public ECommerceContext(DbContextOptions<ECommerceContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<CartItem> CartItems { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<Address> Addresses { get; set; }
    public DbSet<Review> Reviews { get; set; }
    public DbSet<ProductImage> ProductImages { get; set; }
}