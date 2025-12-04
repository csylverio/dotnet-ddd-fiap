namespace FiapEcommerce.Domain.PurchaseTransaction.DomainEvents;

public abstract class BaseOrderEvent
{
    public int OrderId { get; set; }
    public DateTime EventDate { get; set; } = DateTime.UtcNow;
    public string EventId { get; set; } = Guid.NewGuid().ToString();
    public int CustomerId { get; set; }
    public decimal OrderAmount { get; set; }
}

public class OrderCreatedEvent : BaseOrderEvent
{
    public List<OrderItemInfo> Items { get; set; } = new();
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
}

public class OrderStatusChangedEvent : BaseOrderEvent
{
    public OrderStatus PreviousStatus { get; set; }
    public OrderStatus NewStatus { get; set; }
    public string ChangeReason { get; set; } = string.Empty;
    public string ChangedBy { get; set; } = "System";
}

public class PaymentProcessedEvent : BaseOrderEvent
{
    public string PaymentMethod { get; set; } = string.Empty;
    public decimal PaymentAmount { get; set; }
    public bool PaymentSuccess { get; set; }
    public string TransactionId { get; set; } = string.Empty;
    public string PaymentGateway { get; set; } = string.Empty;
}

public class OrderCancelledEvent : BaseOrderEvent
{
    public string CancellationReason { get; set; } = string.Empty;
    public string CancelledBy { get; set; } = "System";
    public bool RefundRequired { get; set; }
    public decimal RefundAmount { get; set; }
}

public class OrderItemInfo
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
}