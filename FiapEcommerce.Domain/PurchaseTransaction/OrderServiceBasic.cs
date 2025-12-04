using System;
using System.Linq;
using FiapEcommerce.Domain.PurchaseTransaction.Composite;
using FiapEcommerce.Domain.PurchaseTransaction.Financial;

namespace FiapEcommerce.Domain.PurchaseTransaction;

/// <summary>
/// Serviço de pedidos "monolítico" utilizado para demonstrar um fluxo acoplado.
/// Toda a lógica de desconto, validação, pagamento e mudança de status fica embutida
/// na própria classe sem o apoio dos padrões comportamentais.
/// </summary>
public class OrderServiceBasic : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IPaymentGatewayFactory _paymentGatewayFactory;
    private readonly IDiscountConfiguration _discountConfiguration;
    private readonly IAccountingService _accountingService;

    public OrderServiceBasic(
        IOrderRepository orderRepository,
        IPaymentRepository paymentRepository,
        IPaymentGatewayFactory paymentGatewayFactory,
        IDiscountConfiguration discountConfiguration,
        IAccountingService accountingService)
    {
        _orderRepository = orderRepository;
        _paymentRepository = paymentRepository;
        _paymentGatewayFactory = paymentGatewayFactory;
        _discountConfiguration = discountConfiguration;
        _accountingService = accountingService;
    }

    public Order GetById(int orderId)
    {
        return _orderRepository.GetById(orderId);
    }

    public Order Create(Order order)
    {
        ValidateOrderInline(order);
        _orderRepository.Add(order);
        return order;
    }

    public Order FinalizeOrder(Order order, string? couponCode = null)
    {
        ValidateOrderInline(order);

        var (baseDiscount, promotionalDiscount) = CalculateDiscountsInline(order, couponCode);
        var totalDiscount = LimitDiscount(order, baseDiscount + promotionalDiscount);

        order.Discount = totalDiscount; // Armazena o valor absoluto do desconto
        order.Status = OrderStatus.AwaitingPayment; // Atualiza status diretamente (sem State)
        order.DiscountDetail = new DiscountDetail
        {
            BaseDiscount = baseDiscount,
            PromotionalDiscount = promotionalDiscount,
            FinalDiscount = totalDiscount
        };

        ApplyAccountingInline(order);
        _orderRepository.Update(order);
        return order;
    }

    public PaymentResult MakePayment(Order order, int paymentMethodId)
    {
        try
        {
            var payment = new Payment
            {
                OrderId = order.Id,
                Amount = order.TotalAmount,
                PaymentMethodId = paymentMethodId,
                Status = PaymentStatus.Pending
            };

            if (!ValidatePaymentInline(payment))
            {
                return new PaymentResult
                {
                    Success = false,
                    Status = "Validação Falhou",
                    ErrorMessage = "Dados do pagamento inválidos",
                    NextStep = OrderStatus.PaymentError
                };
            }

            var gateway = _paymentGatewayFactory.Create(paymentMethodId);
            var gatewayResponse = gateway.ProcessPayment(order.TotalAmount);

            payment.TransactionId = gatewayResponse.GatewayTransactionId;
            payment.GatewayResponse = gatewayResponse.RawResponse;
            payment.Status = gatewayResponse.IsSuccess ? PaymentStatus.Approved : PaymentStatus.Declined;
            payment.ErrorMessage = gatewayResponse.ErrorMessage;

            _paymentRepository.Add(payment);

            if (gatewayResponse.IsSuccess)
            {
                order.Status = OrderStatus.PaymentApproved; // Mudança de status manual
                _orderRepository.Update(order);

                return new PaymentResult
                {
                    Success = true,
                    PaymentId = payment.Id,
                    TransactionId = payment.TransactionId,
                    Amount = payment.Amount,
                    PaymentDate = payment.PaymentDate,
                    PaymentMethod = paymentMethodId,
                    Status = "Aprovado",
                    Message = "Pagamento processado sem padrões",
                    NextStep = order.Status
                };
            }

            return new PaymentResult
            {
                Success = false,
                PaymentId = payment.Id,
                Status = "Recusado",
                ErrorMessage = gatewayResponse.ErrorMessage,
                NextStep = OrderStatus.PaymentFailed
            };
        }
        catch (Exception ex)
        {
            return new PaymentResult
            {
                Success = false,
                Status = "Erro",
                ErrorMessage = ex.Message,
                NextStep = OrderStatus.PaymentError
            };
        }
    }

    private void ValidateOrderInline(Order order)
    {
        if (order.CustomerId <= 0)
        {
            throw new InvalidOperationException("Cliente é obrigatório no fluxo básico");
        }

        if (!order.Items.Any())
        {
            throw new InvalidOperationException("Pedido precisa ter itens antes de ser finalizado");
        }
    }

    private (decimal baseDiscount, decimal promotionalDiscount) CalculateDiscountsInline(Order order, string? couponCode)
    {
        var grossTotal = order.Items.Sum(i => i.TotalPrice);
        decimal baseDiscount = 0m;
        decimal promotionalDiscount = 0m;

        if (order.Customer?.IsFirstPurchase == true)
        {
            baseDiscount += 50m;
        }

        if (order.Items.Count > 10)
        {
            baseDiscount += grossTotal * 0.05m;
        }

        if (_discountConfiguration.ApplyPercentageDiscount)
        {
            baseDiscount += grossTotal * (_discountConfiguration.Percentage / 100m);
        }

        if (_discountConfiguration.ApplyFixedDiscount)
        {
            baseDiscount += _discountConfiguration.FixedAmount;
        }

        var now = DateTime.Now;
        if (now.Month == 11 && now.Day is >= 20 and <= 30)
        {
            promotionalDiscount += grossTotal * 0.15m;
        }

        if (order.Customer?.BirthDate.Month == now.Month)
        {
            promotionalDiscount += 100m;
        }

        if (!string.IsNullOrWhiteSpace(couponCode) && couponCode.Equals("DESC20", StringComparison.OrdinalIgnoreCase))
        {
            promotionalDiscount += grossTotal * 0.20m;
        }

        return (baseDiscount, promotionalDiscount);
    }

    private decimal LimitDiscount(Order order, decimal totalDiscount)
    {
        var grossTotal = order.Items.Sum(i => i.TotalPrice);
        var maxAllowed = grossTotal * 0.30m;
        return Math.Min(totalDiscount, maxAllowed);
    }

    private void ApplyAccountingInline(Order order)
    {
        var accountingResult = _accountingService.RegisterSale(order);
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

    private bool ValidatePaymentInline(Payment payment)
    {
        return payment.Amount > 0 && payment.PaymentMethodId > 0;
    }
}
