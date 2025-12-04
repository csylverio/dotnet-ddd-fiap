namespace FiapEcommerce.Domain.PurchaseTransaction.DomainEvents;

/// <summary>
/// Publisher de eventos de domínio que implementa o padrão Observer.
/// Centraliza o registro e notificação dos subscribers do domínio de pedidos.
/// Colabora com o serviço de pedidos para disparar eventos ricos durante o fluxo.
/// </summary>
public class OrderEventPublisher : IOrderEventPublisher
{
    private readonly List<IOrderEventSubscriber> _subscribers = new();
    private readonly ILogger<OrderEventPublisher>? _logger;

    public OrderEventPublisher(ILogger<OrderEventPublisher>? logger = null)
    {
        _logger = logger;
    }

    #region Métodos públicos (Observer)

    public void Subscribe(IOrderEventSubscriber subscriber)
    {
        if (!_subscribers.Contains(subscriber))
        {
            _subscribers.Add(subscriber);
            _logger?.LogInformation($"Subscriber {subscriber.GetType().Name} adicionado");
        }
    }

    public void Unsubscribe(IOrderEventSubscriber subscriber)
    {
        if (_subscribers.Remove(subscriber))
        {
            _logger?.LogInformation($"Subscriber {subscriber.GetType().Name} removido");
        }
    }

    public async Task PublishOrderCreatedAsync(OrderCreatedEvent orderEvent)
    {
        _logger?.LogInformation($"Publicando evento OrderCreated para pedido {orderEvent.OrderId}");
        await NotifySubscribers(async subscriber => await subscriber.OnOrderCreatedAsync(orderEvent));
    }

    public async Task PublishOrderStatusChangedAsync(OrderStatusChangedEvent orderEvent)
    {
        _logger?.LogInformation($"Publicando evento OrderStatusChanged para pedido {orderEvent.OrderId}: {orderEvent.PreviousStatus} -> {orderEvent.NewStatus}");
        await NotifySubscribers(async subscriber => await subscriber.OnOrderStatusChangedAsync(orderEvent));
    }

    public async Task PublishPaymentProcessedAsync(PaymentProcessedEvent orderEvent)
    {
        _logger?.LogInformation($"Publicando evento PaymentProcessed para pedido {orderEvent.OrderId}");
        await NotifySubscribers(async subscriber => await subscriber.OnPaymentProcessedAsync(orderEvent));
    }

    public async Task PublishOrderCancelledAsync(OrderCancelledEvent orderEvent)
    {
        _logger?.LogInformation($"Publicando evento OrderCancelled para pedido {orderEvent.OrderId}");
        await NotifySubscribers(async subscriber => await subscriber.OnOrderCancelledAsync(orderEvent));
    }

    #endregion

    private async Task NotifySubscribers(Func<IOrderEventSubscriber, Task> action)
    {
        var tasks = _subscribers.Select(async subscriber =>
        {
            try
            {
                await action(subscriber);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Erro ao notificar subscriber {subscriber.GetType().Name}");
            }
        });

        await Task.WhenAll(tasks);
    }
}

// Interface para logging (caso não tenha Microsoft.Extensions.Logging)
public interface ILogger<T>
{
    void LogInformation(string message);
    void LogError(Exception ex, string message);
}

// Implementação simples de logger caso não tenha o Microsoft.Extensions.Logging
public class SimpleLogger<T> : ILogger<T>
{
    public void LogInformation(string message)
    {
        Console.WriteLine($"[INFO] {DateTime.Now:HH:mm:ss} - {message}");
    }

    public void LogError(Exception ex, string message)
    {
        Console.WriteLine($"[ERROR] {DateTime.Now:HH:mm:ss} - {message}: {ex.Message}");
    }
}
