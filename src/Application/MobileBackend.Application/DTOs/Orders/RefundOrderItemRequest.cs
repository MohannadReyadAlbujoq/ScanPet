namespace MobileBackend.Application.DTOs.Orders;

public class RefundOrderItemRequest
{
    public int RefundQuantity { get; set; }
    public string? RefundReason { get; set; }
    public Guid RefundToInventoryId { get; set; }
}
