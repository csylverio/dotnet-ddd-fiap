namespace FiapEcommerce.Domain.PurchaseTransaction.DomainEvents.Subscribers;

public class AuditLogSubscriber : IOrderEventSubscriber
{
    private readonly IAuditLogService _auditLogService;

    public AuditLogSubscriber(IAuditLogService auditLogService)
    {
        _auditLogService = auditLogService;
    }

    public async Task OnOrderCreatedAsync(OrderCreatedEvent orderEvent)
    {
        var auditLog = new AuditLogEntry
        {
            EventType = "OrderCreated",
            EntityType = "Order",
            EntityId = orderEvent.OrderId.ToString(),
            EventData = new
            {
                CustomerId = orderEvent.CustomerId,
                OrderAmount = orderEvent.OrderAmount,
                ItemCount = orderEvent.Items.Count,
                Items = orderEvent.Items.Select(i => new { i.ProductId, i.Quantity, i.UnitPrice })
            },
            EventDate = orderEvent.EventDate,
            EventId = orderEvent.EventId
        };

        await _auditLogService.LogAsync(auditLog);
        Console.WriteLine($"📝 Audit log registrado - Pedido criado #{orderEvent.OrderId}");
    }

    public async Task OnOrderStatusChangedAsync(OrderStatusChangedEvent orderEvent)
    {
        var auditLog = new AuditLogEntry
        {
            EventType = "OrderStatusChanged",
            EntityType = "Order",
            EntityId = orderEvent.OrderId.ToString(),
            EventData = new
            {
                PreviousStatus = orderEvent.PreviousStatus.ToString(),
                NewStatus = orderEvent.NewStatus.ToString(),
                ChangeReason = orderEvent.ChangeReason,
                ChangedBy = orderEvent.ChangedBy,
                CustomerId = orderEvent.CustomerId,
                OrderAmount = orderEvent.OrderAmount
            },
            EventDate = orderEvent.EventDate,
            EventId = orderEvent.EventId
        };

        await _auditLogService.LogAsync(auditLog);
        Console.WriteLine($"📝 Audit log registrado - Status alterado #{orderEvent.OrderId}: {orderEvent.PreviousStatus} -> {orderEvent.NewStatus}");
    }

    public async Task OnPaymentProcessedAsync(PaymentProcessedEvent orderEvent)
    {
        var auditLog = new AuditLogEntry
        {
            EventType = "PaymentProcessed",
            EntityType = "Payment",
            EntityId = orderEvent.TransactionId,
            EventData = new
            {
                OrderId = orderEvent.OrderId,
                PaymentAmount = orderEvent.PaymentAmount,
                PaymentMethod = orderEvent.PaymentMethod,
                PaymentSuccess = orderEvent.PaymentSuccess,
                PaymentGateway = orderEvent.PaymentGateway,
                CustomerId = orderEvent.CustomerId
            },
            EventDate = orderEvent.EventDate,
            EventId = orderEvent.EventId
        };

        await _auditLogService.LogAsync(auditLog);
        Console.WriteLine($"📝 Audit log registrado - Pagamento processado #{orderEvent.OrderId}: {(orderEvent.PaymentSuccess ? "Sucesso" : "Falha")}");
    }

    public async Task OnOrderCancelledAsync(OrderCancelledEvent orderEvent)
    {
        var auditLog = new AuditLogEntry
        {
            EventType = "OrderCancelled",
            EntityType = "Order",
            EntityId = orderEvent.OrderId.ToString(),
            EventData = new
            {
                CancellationReason = orderEvent.CancellationReason,
                CancelledBy = orderEvent.CancelledBy,
                RefundRequired = orderEvent.RefundRequired,
                RefundAmount = orderEvent.RefundAmount,
                CustomerId = orderEvent.CustomerId,
                OrderAmount = orderEvent.OrderAmount
            },
            EventDate = orderEvent.EventDate,
            EventId = orderEvent.EventId
        };

        await _auditLogService.LogAsync(auditLog);
        Console.WriteLine($"📝 Audit log registrado - Pedido cancelado #{orderEvent.OrderId}: {orderEvent.CancellationReason}");
    }
}

// Modelo de audit log
public class AuditLogEntry
{
    public string EventType { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public object EventData { get; set; } = new();
    public DateTime EventDate { get; set; }
    public string EventId { get; set; } = string.Empty;
    public string UserId { get; set; } = "System";
    public string IPAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
}

// Interface do serviço de audit log
public interface IAuditLogService
{
    Task LogAsync(AuditLogEntry auditLog);
    Task<List<AuditLogEntry>> GetLogsByEntityAsync(string entityType, string entityId);
    Task<List<AuditLogEntry>> GetLogsByDateRangeAsync(DateTime startDate, DateTime endDate);
}

// Implementação simulada do serviço de audit log
public class AuditLogService : IAuditLogService
{
    private readonly List<AuditLogEntry> _auditLogs = new();

    public async Task LogAsync(AuditLogEntry auditLog)
    {
        await Task.Delay(10); // Simula latência de escrita
        _auditLogs.Add(auditLog);
    }

    public async Task<List<AuditLogEntry>> GetLogsByEntityAsync(string entityType, string entityId)
    {
        await Task.Delay(10);
        return _auditLogs
            .Where(log => log.EntityType == entityType && log.EntityId == entityId)
            .OrderByDescending(log => log.EventDate)
            .ToList();
    }

    public async Task<List<AuditLogEntry>> GetLogsByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        await Task.Delay(10);
        return _auditLogs
            .Where(log => log.EventDate >= startDate && log.EventDate <= endDate)
            .OrderByDescending(log => log.EventDate)
            .ToList();
    }
}