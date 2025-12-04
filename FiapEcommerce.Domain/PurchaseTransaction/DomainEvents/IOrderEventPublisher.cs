namespace FiapEcommerce.Domain.PurchaseTransaction.DomainEvents;

public interface IOrderEventPublisher
{
    void Subscribe(IOrderEventSubscriber subscriber);
    void Unsubscribe(IOrderEventSubscriber subscriber);
    Task PublishOrderCreatedAsync(OrderCreatedEvent orderEvent);
    Task PublishOrderStatusChangedAsync(OrderStatusChangedEvent orderEvent);
    Task PublishPaymentProcessedAsync(PaymentProcessedEvent orderEvent);
    Task PublishOrderCancelledAsync(OrderCancelledEvent orderEvent);
}

public interface IOrderEventSubscriber
{
    Task OnOrderCreatedAsync(OrderCreatedEvent orderEvent);
    Task OnOrderStatusChangedAsync(OrderStatusChangedEvent orderEvent);
    Task OnPaymentProcessedAsync(PaymentProcessedEvent orderEvent);
    Task OnOrderCancelledAsync(OrderCancelledEvent orderEvent);
}