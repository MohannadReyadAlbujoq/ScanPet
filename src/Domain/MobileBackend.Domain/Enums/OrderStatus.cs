namespace MobileBackend.Domain.Enums;

public enum OrderStatus
{
    Pending = 0,
    Confirmed = 1,
    Cancelled = 2,
    Refunded = 3,
    PartiallyRefunded = 4
}
