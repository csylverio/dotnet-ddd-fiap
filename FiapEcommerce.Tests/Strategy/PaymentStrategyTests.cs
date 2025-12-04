using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FiapEcommerce.Domain.InventoryManagement;
using FiapEcommerce.Domain.PurchaseTransaction;
using FiapEcommerce.Domain.PurchaseTransaction.Financial;
using FiapEcommerce.Domain.PurchaseTransaction.Strategy;
using Xunit;

namespace FiapEcommerce.Tests.Strategy;

public class PaymentStrategyTests
{
    [Fact]
    public async Task Deve_Processar_Pagamento_A_Vista_Com_Strategy()
    {
        var context = BuildStrategyContext();
        var order = CreateOrder();
        var payment = new Payment
        {
            PaymentMethodId = 1,
            Amount = 500,
            CardNumber = "4111111111111111"
        };

        var result = await context.ProcessPaymentAsync(order, payment, PaymentType.SinglePayment);

        Assert.True(result.Success);
        Assert.Equal(OrderStatus.PaymentApproved, result.NextStep);
    }

    [Fact]
    public async Task Deve_Processar_Pagamento_Parcelado_Com_Strategy()
    {
        var context = BuildStrategyContext();
        var order = CreateOrder();
        var payment = new Payment
        {
            PaymentMethodId = 2,
            Amount = 500,
            CardNumber = "4111111111111111",
            InstallmentCount = 3
        };

        var result = await context.ProcessPaymentAsync(order, payment, PaymentType.InstallmentPayment);

        Assert.True(result.Success);
        Assert.Equal(OrderStatus.PaymentApproved, result.NextStep);
        Assert.Contains("parcelado", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    private static PaymentStrategyContext BuildStrategyContext()
    {
        var factory = new FakeGatewayFactory();
        var strategies = new List<IPaymentProcessingStrategy>
        {
            new SinglePaymentStrategy(factory),
            new InstallmentPaymentStrategy(factory)
        };

        return new PaymentStrategyContext(strategies);
    }

    private static Order CreateOrder()
    {
        var product = new Product { Id = 1, SalePrice = 250m, Name = "Mouse" };
        var order = new Order
        {
            CustomerId = 1,
            Items = new List<Item>
            {
                new(product, 2)
            }
        };

        return order;
    }

    private sealed class FakeGatewayFactory : IPaymentGatewayFactory
    {
        public IPaymentGateway Create(int paymentMethodId) => new FakeGateway();
    }

    private sealed class FakeGateway : IPaymentGateway
    {
        public PaymentGatewayResponse ProcessPayment(decimal amount)
        {
            return new PaymentGatewayResponse
            {
                GatewayTransactionId = Guid.NewGuid().ToString(),
                IsSuccess = true,
                ProcessedAmount = amount
            };
        }

        public Task<PaymentGatewayResponse> ProcessPaymentAsync(Payment payment)
        {
            return Task.FromResult(ProcessPayment(payment.Amount));
        }
    }
}
