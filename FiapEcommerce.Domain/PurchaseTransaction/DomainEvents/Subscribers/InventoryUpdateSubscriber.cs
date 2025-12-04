using FiapEcommerce.Domain.InventoryManagement;

namespace FiapEcommerce.Domain.PurchaseTransaction.DomainEvents.Subscribers;

public class InventoryUpdateSubscriber : IOrderEventSubscriber
{
    private readonly IProductRepository _productRepository;

    public InventoryUpdateSubscriber(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task OnOrderCreatedAsync(OrderCreatedEvent orderEvent)
    {
        // Reservar itens no estoque quando pedido é criado
        foreach (var item in orderEvent.Items)
        {
            await ReserveInventory(item.ProductId, item.Quantity);
            Console.WriteLine($"📦 Estoque reservado - Produto {item.ProductId}: {item.Quantity} unidades");
        }
    }

    public async Task OnOrderStatusChangedAsync(OrderStatusChangedEvent orderEvent)
    {
        // Atualizar estoque baseado na mudança de status
        switch (orderEvent.NewStatus)
        {
            case OrderStatus.PaymentApproved:
                await ConfirmInventoryReservation(orderEvent.OrderId);
                Console.WriteLine($"📦 Reserva de estoque confirmada para pedido {orderEvent.OrderId}");
                break;

            case OrderStatus.Canceled:
                await ReleaseInventoryReservation(orderEvent.OrderId);
                Console.WriteLine($"📦 Reserva de estoque liberada para pedido {orderEvent.OrderId}");
                break;

            case OrderStatus.Shipped:
                await UpdateInventoryAfterShipment(orderEvent.OrderId);
                Console.WriteLine($"📦 Estoque atualizado após envio do pedido {orderEvent.OrderId}");
                break;
        }
    }

    public async Task OnPaymentProcessedAsync(PaymentProcessedEvent orderEvent)
    {
        if (!orderEvent.PaymentSuccess)
        {
            // Liberar reserva se pagamento falhou
            await ReleaseInventoryReservation(orderEvent.OrderId);
            Console.WriteLine($"📦 Reserva de estoque liberada devido a falha no pagamento - Pedido {orderEvent.OrderId}");
        }
    }

    public async Task OnOrderCancelledAsync(OrderCancelledEvent orderEvent)
    {
        // Restaurar estoque quando pedido é cancelado
        await ReleaseInventoryReservation(orderEvent.OrderId);
        Console.WriteLine($"📦 Estoque restaurado devido ao cancelamento - Pedido {orderEvent.OrderId}");
    }

    private async Task ReserveInventory(int productId, int quantity)
    {
        // Simulação de reserva de estoque
        await Task.Delay(50);
        
        // Em um sistema real, isso atualizaria uma tabela de reservas
        // ou decrementaria o estoque disponível
    }

    private async Task ConfirmInventoryReservation(int orderId)
    {
        // Simulação de confirmação de reserva
        await Task.Delay(50);
        
        // Em um sistema real, isso moveria itens de "reservado" para "vendido"
    }

    private async Task ReleaseInventoryReservation(int orderId)
    {
        // Simulação de liberação de reserva
        await Task.Delay(50);
        
        // Em um sistema real, isso liberaria os itens reservados de volta ao estoque disponível
    }

    private async Task UpdateInventoryAfterShipment(int orderId)
    {
        // Simulação de atualização final do estoque
        await Task.Delay(50);
        
        // Em um sistema real, isso finalizaria a movimentação do estoque
        // e poderia disparar reposições se necessário
    }
}