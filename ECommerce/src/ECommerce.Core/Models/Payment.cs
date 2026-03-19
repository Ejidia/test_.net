using ECommerce.Core.Enums;

namespace ECommerce.Core.Models;

public class Payment
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty; // CreditCard, PayPal, etc.
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public string? TransactionId { get; set; }
    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
    public string? PaymentGatewayResponse { get; set; }

    // Navigation properties
    public Order Order { get; set; } = null!;
}
