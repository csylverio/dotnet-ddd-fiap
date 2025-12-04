namespace FiapEcommerce.Domain.PurchaseTransaction.State;

/// <summary>
/// Máquina de estados que representa o padrão State aplicado aos pedidos.
/// Centraliza as transições e ações permitidas, mantendo o domínio coerente.
/// É utilizada por <see cref="Order"/> e pelos serviços para executar validações de status.
/// </summary>
public class OrderStateMachine
{
    private static readonly Dictionary<OrderStatus, List<OrderStatus>> _allowedTransitions = new()
    {
        { OrderStatus.Draft, new List<OrderStatus> { OrderStatus.AwaitingPayment, OrderStatus.Canceled } },
        { OrderStatus.AwaitingPayment, new List<OrderStatus> { OrderStatus.PaymentApproved, OrderStatus.PaymentError, OrderStatus.Canceled } },
        { OrderStatus.PaymentApproved, new List<OrderStatus> { OrderStatus.Processing, OrderStatus.Canceled } },
        { OrderStatus.Processing, new List<OrderStatus> { OrderStatus.Shipped, OrderStatus.Canceled } },
        { OrderStatus.Shipped, new List<OrderStatus> { OrderStatus.Delivered } },
        { OrderStatus.Delivered, new List<OrderStatus>() }, // Estado final
        { OrderStatus.Canceled, new List<OrderStatus>() }, // Estado final
        { OrderStatus.PaymentError, new List<OrderStatus> { OrderStatus.AwaitingPayment, OrderStatus.Canceled } }
    };

    private static readonly Dictionary<OrderStatus, List<string>> _allowedActions = new()
    {
        { OrderStatus.Draft, new List<string> { "AddItems", "RemoveItems", "ApplyDiscount", "ProcessPayment", "Cancel" } },
        { OrderStatus.AwaitingPayment, new List<string> { "ProcessPayment", "Cancel" } },
        { OrderStatus.PaymentApproved, new List<string> { "StartProcessing", "Cancel" } },
        { OrderStatus.Processing, new List<string> { "Ship", "Cancel" } },
        { OrderStatus.Shipped, new List<string> { "UpdateTracking", "MarkAsDelivered" } },
        { OrderStatus.Delivered, new List<string> { "RequestReturn" } },
        { OrderStatus.Canceled, new List<string>() },
        { OrderStatus.PaymentError, new List<string> { "RetryPayment", "Cancel" } }
    };

    #region Métodos do padrão State

    public static bool CanTransition(OrderStatus from, OrderStatus to)
    {
        return _allowedTransitions.ContainsKey(from) && _allowedTransitions[from].Contains(to);
    }

    public static List<OrderStatus> GetAllowedNextStates(OrderStatus currentStatus)
    {
        return _allowedTransitions.ContainsKey(currentStatus) ? _allowedTransitions[currentStatus] : new List<OrderStatus>();
    }

    public static List<string> GetAllowedActions(OrderStatus currentStatus)
    {
        return _allowedActions.ContainsKey(currentStatus) ? _allowedActions[currentStatus] : new List<string>();
    }

    public static OrderTransitionResult ValidateAndTransition(Order order, OrderStatus newStatus, string reason = "")
    {
        if (!CanTransition(order.Status, newStatus))
        {
            return OrderTransitionResult.CreateFailure($"Transição inválida de {order.Status} para {newStatus}");
        }

        var previousStatus = order.Status;
        order.UpdateStatus(newStatus);

        return OrderTransitionResult.CreateSuccess(previousStatus, newStatus, reason);
    }

    #endregion
}

public class OrderTransitionResult
{
    public bool Success { get; private set; }
    public string Message { get; private set; } = string.Empty;
    public OrderStatus? PreviousStatus { get; private set; }
    public OrderStatus? NewStatus { get; private set; }
    public DateTime TransitionDate { get; private set; }
    public string Reason { get; private set; } = string.Empty;

    private OrderTransitionResult() { }

    public static OrderTransitionResult CreateSuccess(OrderStatus previousStatus, OrderStatus newStatus, string reason = "")
    {
        return new OrderTransitionResult
        {
            Success = true,
            Message = $"Status alterado de {previousStatus} para {newStatus}",
            PreviousStatus = previousStatus,
            NewStatus = newStatus,
            TransitionDate = DateTime.UtcNow,
            Reason = reason
        };
    }

    public static OrderTransitionResult CreateFailure(string message)
    {
        return new OrderTransitionResult
        {
            Success = false,
            Message = message,
            TransitionDate = DateTime.UtcNow
        };
    }
}
