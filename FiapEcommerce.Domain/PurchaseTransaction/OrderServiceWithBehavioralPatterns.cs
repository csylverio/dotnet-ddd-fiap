using FiapEcommerce.Domain.PurchaseTransaction.Composite;
using FiapEcommerce.Domain.PurchaseTransaction.Composite.Rules;
using FiapEcommerce.Domain.PurchaseTransaction.Financial;
using FiapEcommerce.Domain.PurchaseTransaction.Strategy;
using FiapEcommerce.Domain.PurchaseTransaction.State;
using FiapEcommerce.Domain.PurchaseTransaction.ChainOfResponsibility;
using FiapEcommerce.Domain.PurchaseTransaction.DomainEvents;
using FiapEcommerce.Domain.CustomerRelationshipManagement;
using FiapEcommerce.Domain.InventoryManagement;

namespace FiapEcommerce.Domain.PurchaseTransaction;

/// <summary>
/// Serviço orientado a padrões comportamentais que demonstra Strategy, Composite, Chain of Responsibility,
/// Observer e State atuando em conjunto sobre o agregador de pedidos.
/// Ele continua atendendo <see cref="IOrderService"/> para possibilitar a comparação direta com o serviço básico.
/// </summary>
public class OrderServiceWithBehavioralPatterns : IOrderService
{
    #region Dependências

    private readonly IOrderRepository _orderRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IDiscountConfiguration _discountConfiguration;
    private readonly IAccountingService _accountingService;
    
    // Novos padrões comportamentais
    private readonly PaymentStrategyContext _paymentStrategyContext;
    private readonly OrderProcessingChain _orderProcessingChain;
    private readonly IOrderEventPublisher _eventPublisher;

    #endregion

    public OrderServiceWithBehavioralPatterns(
        IOrderRepository orderRepository, 
        IPaymentRepository paymentRepository,
        IDiscountConfiguration discountConfiguration,
        IAccountingService accountingService,
        PaymentStrategyContext paymentStrategyContext,
        OrderProcessingChain orderProcessingChain,
        IOrderEventPublisher eventPublisher)
    {
        _orderRepository = orderRepository;
        _paymentRepository = paymentRepository;
        _discountConfiguration = discountConfiguration;
        _accountingService = accountingService;
        _paymentStrategyContext = paymentStrategyContext;
        _orderProcessingChain = orderProcessingChain;
        _eventPublisher = eventPublisher;
    }

    #region Métodos públicos principais

    public Order GetById(int orderId)
    {
        return _orderRepository.GetById(orderId);
    }

    public async Task<Order> CreateAsync(Order order)
    {
        // Chain of Responsibility - Validar pedido antes da criação
        var validationResult = await _orderProcessingChain.ProcessOrderAsync(order, "create");
        
        if (!validationResult.Success)
        {
            throw new InvalidOperationException($"Falha na validação do pedido: {validationResult.Message}");
        }

        _orderRepository.Add(order);

        // Observer Pattern - Publicar evento de pedido criado
        var orderCreatedEvent = CreateOrderCreatedEvent(order);
        await _eventPublisher.PublishOrderCreatedAsync(orderCreatedEvent);

        return order;
    }

    public async Task<Order> FinalizeOrderAsync(Order order, string? couponCode = null)
    {
        // Chain of Responsibility - Validação completa antes de finalizar
        var validationResult = await _orderProcessingChain.ProcessOrderAsync(order, "finalize");
        
        if (!validationResult.Success)
        {
            throw new InvalidOperationException($"Falha na validação do pedido: {validationResult.Message}");
        }

        // Composite - calcula descontos a partir da composição de regras
        ApplyDiscounts(order, couponCode);

        // State Pattern - Transição de status controlada pela máquina de estados
        var statusChangeResult = order.ChangeStatus(OrderStatus.AwaitingPayment, "Pedido finalizado e aguardando pagamento");
        
        if (!statusChangeResult.Success)
        {
            throw new InvalidOperationException($"Erro na mudança de status: {statusChangeResult.Message}");
        }

        // Registrar na contabilidade
        var accountingResult = _accountingService.RegisterSale(order);
        UpdateAccountingStatus(order, accountingResult);

        _orderRepository.Update(order);

        // Observer Pattern - Publicar evento de mudança de status
        var statusChangedEvent = CreateOrderStatusChangedEvent(
            order,
            statusChangeResult.PreviousStatus ?? OrderStatus.Draft,
            statusChangeResult.NewStatus ?? OrderStatus.AwaitingPayment,
            "Pedido finalizado");
        await _eventPublisher.PublishOrderStatusChangedAsync(statusChangedEvent);

        return order;
    }

    public async Task<PaymentResult> MakePaymentAsync(Order order, Payment payment, PaymentType paymentType)
    {
        try
        {
            // Verificar se o pedido pode receber pagamento (State Pattern)
            if (!order.GetAllowedActions().Contains("ProcessPayment"))
            {
                return new PaymentResult
                {
                    Success = false,
                    Status = "Erro",
                    ErrorMessage = $"Pedido no status {order.Status} não pode receber pagamento",
                    NextStep = order.Status
                };
            }

            // Strategy Pattern - Usar estratégia de pagamento apropriada
            var paymentResult = await _paymentStrategyContext.ProcessPaymentAsync(order, payment, paymentType);

            // Atualizar status do pedido baseado no resultado do pagamento
            if (paymentResult.Success && paymentResult.NextStep.HasValue)
            {
                var statusChangeResult = order.ChangeStatus(paymentResult.NextStep.Value, "Pagamento processado");
                
                if (statusChangeResult.Success)
                {
                    _orderRepository.Update(order);
                    
                    // Observer Pattern - Publicar eventos
                    var paymentEvent = CreatePaymentProcessedEvent(order, payment, paymentResult);
                    await _eventPublisher.PublishPaymentProcessedAsync(paymentEvent);

                    var statusEvent = CreateOrderStatusChangedEvent(
                        order,
                        statusChangeResult.PreviousStatus ?? order.Status,
                        statusChangeResult.NewStatus ?? paymentResult.NextStep.Value,
                        "Pagamento processado");
                    await _eventPublisher.PublishOrderStatusChangedAsync(statusEvent);
                }
            }

            _paymentRepository.Add(payment);
            return paymentResult;
        }
        catch (Exception ex)
        {
            // Observer Pattern - Publicar evento de falha no pagamento
            var paymentEvent = CreatePaymentProcessedEvent(order, payment, new PaymentResult 
            { 
                Success = false, 
                ErrorMessage = ex.Message 
            });
            await _eventPublisher.PublishPaymentProcessedAsync(paymentEvent);

            return new PaymentResult
            {
                Success = false,
                Status = "Erro",
                ErrorMessage = "Erro interno no processamento do pagamento",
                NextStep = OrderStatus.PaymentError
            };
        }
    }

    public async Task<bool> CancelOrderAsync(Order order, string reason)
    {
        // State Pattern - Verificar se pode ser cancelado
        if (!order.CanTransitionTo(OrderStatus.Canceled))
        {
            return false;
        }

        var statusChangeResult = order.ChangeStatus(OrderStatus.Canceled, reason);
        
        if (statusChangeResult.Success)
        {
            _orderRepository.Update(order);

            // Observer Pattern - Publicar evento de cancelamento
            var cancelledEvent = CreateOrderCancelledEvent(order, reason);
            await _eventPublisher.PublishOrderCancelledAsync(cancelledEvent);

            return true;
        }

        return false;
    }

    // Método original mantido para compatibilidade
    public Order Create(Order order)
    {
        return CreateAsync(order).GetAwaiter().GetResult();
    }

    public Order FinalizeOrder(Order order, string? couponCode = null)
    {
        return FinalizeOrderAsync(order, couponCode).GetAwaiter().GetResult();
    }

    public PaymentResult MakePayment(Order order, int paymentMethodId)
    {
        var payment = new Payment
        {
            OrderId = order.Id,
            Amount = order.TotalAmount,
            PaymentMethodId = paymentMethodId,
            Status = PaymentStatus.Pending
        };

        // Assumir pagamento à vista como padrão
        return MakePaymentAsync(order, payment, PaymentType.SinglePayment).GetAwaiter().GetResult();
    }

    #endregion

    #region Private Helper Methods

    private void ApplyDiscounts(Order order, string? couponCode)
    {
        // Mantém a lógica original de desconto usando Composite Pattern
        var baseDiscounts = new BaseDiscountsComposite();
        var promotionalDiscounts = new PromotionalDiscountsComposite(order);

        if (_discountConfiguration.ApplyPercentageDiscount)
        {
            baseDiscounts.AddRule(new PercentageDiscountLeaf(_discountConfiguration.Percentage));
        }

        if (_discountConfiguration.ApplyFixedDiscount)
        {
            baseDiscounts.AddRule(new FixedAmountDiscountLeaf(_discountConfiguration.FixedAmount));
        }

        if (!string.IsNullOrEmpty(couponCode))
        {
            promotionalDiscounts.AddTemporaryPromotion(new CouponDiscountLeaf(couponCode));
        }

        decimal baseDiscount = baseDiscounts.CalculateDiscount(order);
        decimal promotionalDiscount = promotionalDiscounts.CalculateDiscount(order);
        decimal totalDiscount = baseDiscount + promotionalDiscount;

        decimal maxAllowedDiscount = order.TotalAmount * 0.3m;
        totalDiscount = Math.Min(totalDiscount, maxAllowedDiscount);

        order.Discount = totalDiscount; // mantém o valor absoluto para comparar com o serviço básico
        order.DiscountDetail = new DiscountDetail
        {
            BaseDiscount = baseDiscount,
            PromotionalDiscount = promotionalDiscount,
            FinalDiscount = totalDiscount
        };
    }

    private void UpdateAccountingStatus(Order order, AccountingResult accountingResult)
    {
        if (accountingResult.Success)
        {
            order.AccountingStatus = AccountingStatus.Registered;
            order.AccountingDocument = accountingResult.DocumentNumber;
            order.AccountingDate = accountingResult.AccountingDate;
        }
        else
        {
            order.AccountingStatus = AccountingStatus.Error;
            order.AccountingMessage = accountingResult.Message;
        }
    }

    private OrderCreatedEvent CreateOrderCreatedEvent(Order order)
    {
        return new OrderCreatedEvent
        {
            OrderId = order.Id,
            CustomerId = order.CustomerId,
            OrderAmount = order.TotalAmount,
            CustomerEmail = $"customer{order.CustomerId}@email.com", // Simulado
            CustomerName = order.Customer?.Name ?? "Cliente",
            Items = order.Items.Select(i => new OrderItemInfo
            {
                ProductId = i.Product?.Id ?? 0,
                ProductName = i.Product?.Name ?? $"Produto {i.Product?.Id ?? 0}",
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                TotalPrice = i.TotalPrice
            }).ToList()
        };
    }

    private OrderStatusChangedEvent CreateOrderStatusChangedEvent(Order order, OrderStatus previousStatus, OrderStatus newStatus, string reason)
    {
        return new OrderStatusChangedEvent
        {
            OrderId = order.Id,
            CustomerId = order.CustomerId,
            OrderAmount = order.TotalAmount,
            PreviousStatus = previousStatus,
            NewStatus = newStatus,
            ChangeReason = reason
        };
    }

    private PaymentProcessedEvent CreatePaymentProcessedEvent(Order order, Payment payment, PaymentResult result)
    {
        return new PaymentProcessedEvent
        {
            OrderId = order.Id,
            CustomerId = order.CustomerId,
            OrderAmount = order.TotalAmount,
            PaymentAmount = payment.Amount,
            PaymentMethod = $"Método {payment.PaymentMethodId}",
            PaymentSuccess = result.Success,
            TransactionId = result.TransactionId ?? payment.TransactionId ?? "",
            PaymentGateway = "Gateway Simulado"
        };
    }

    private OrderCancelledEvent CreateOrderCancelledEvent(Order order, string reason)
    {
        return new OrderCancelledEvent
        {
            OrderId = order.Id,
            CustomerId = order.CustomerId,
            OrderAmount = order.TotalAmount,
            CancellationReason = reason,
            RefundRequired = order.Payments.Any(p => p.Status == PaymentStatus.Approved),
            RefundAmount = order.Payments.Where(p => p.Status == PaymentStatus.Approved).Sum(p => p.Amount)
        };
    }

    #endregion
}
