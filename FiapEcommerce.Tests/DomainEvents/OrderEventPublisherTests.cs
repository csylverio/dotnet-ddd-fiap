using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FiapEcommerce.Domain.InventoryManagement;
using FiapEcommerce.Domain.PurchaseTransaction;
using FiapEcommerce.Domain.PurchaseTransaction.DomainEvents;
using FiapEcommerce.Domain.PurchaseTransaction.DomainEvents.Subscribers;
using Xunit;

namespace FiapEcommerce.Tests.DomainEvents;

public class OrderEventPublisherTests
{
    [Fact]
    public async Task Deve_Notificar_Todos_Subscribers_Do_Observer()
    {
        var emailService = new TestEmailService();
        var auditService = new TestAuditLogService();
        var productRepository = new FakeProductRepository();

        var publisher = new OrderEventPublisher();
        publisher.Subscribe(new EmailNotificationSubscriber(emailService));
        publisher.Subscribe(new InventoryUpdateSubscriber(productRepository));
        publisher.Subscribe(new AuditLogSubscriber(auditService));

        var orderCreated = new OrderCreatedEvent
        {
            OrderId = 1,
            CustomerId = 1,
            CustomerEmail = "teste@email.com",
            CustomerName = "Cliente",
            OrderAmount = 500m,
            Items = new List<OrderItemInfo>
            {
                new() { ProductId = 1, ProductName = "Mouse", Quantity = 2, TotalPrice = 200m, UnitPrice = 100m }
            }
        };

        await publisher.PublishOrderCreatedAsync(orderCreated);

        var statusChanged = new OrderStatusChangedEvent
        {
            OrderId = 1,
            CustomerId = 1,
            OrderAmount = 500m,
            PreviousStatus = OrderStatus.AwaitingPayment,
            NewStatus = OrderStatus.PaymentApproved,
            ChangeReason = "Pagamento confirmado"
        };

        await publisher.PublishOrderStatusChangedAsync(statusChanged);

        var paymentEvent = new PaymentProcessedEvent
        {
            OrderId = 1,
            CustomerId = 1,
            OrderAmount = 500m,
            PaymentAmount = 500m,
            PaymentMethod = "Cartão",
            PaymentSuccess = true,
            TransactionId = "ABC123"
        };

        await publisher.PublishPaymentProcessedAsync(paymentEvent);

        var cancelledEvent = new OrderCancelledEvent
        {
            OrderId = 1,
            CustomerId = 1,
            OrderAmount = 500m,
            CancellationReason = "Solicitado pelo cliente"
        };

        await publisher.PublishOrderCancelledAsync(cancelledEvent);

        Assert.NotEmpty(emailService.Messages);
        Assert.NotEmpty(auditService.Logs);
    }

    private sealed class TestEmailService : IEmailService
    {
        public List<string> Messages { get; } = new();

        public Task SendEmailAsync(string toEmail, string subject, string body)
        {
            Messages.Add(subject);
            return Task.CompletedTask;
        }
    }

    private sealed class TestAuditLogService : IAuditLogService
    {
        public List<AuditLogEntry> Logs { get; } = new();

        public Task LogAsync(AuditLogEntry auditLog)
        {
            Logs.Add(auditLog);
            return Task.CompletedTask;
        }

        public Task<List<AuditLogEntry>> GetLogsByEntityAsync(string entityType, string entityId)
        {
            return Task.FromResult(new List<AuditLogEntry>());
        }

        public Task<List<AuditLogEntry>> GetLogsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return Task.FromResult(new List<AuditLogEntry>());
        }
    }

    private sealed class FakeProductRepository : IProductRepository
    {
        public Product GetById(int productId)
        {
            return new Product
            {
                Id = productId,
                Name = "Mouse",
                Active = true,
                SalePrice = 100m
            };
        }
    }
}
