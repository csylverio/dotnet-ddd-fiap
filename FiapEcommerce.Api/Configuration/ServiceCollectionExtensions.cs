using LibraryExternal.SAP;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using FiapEcommerce.Domain.CustomerRelationshipManagement;
using FiapEcommerce.Domain.InventoryManagement;
using FiapEcommerce.Domain.PurchaseTransaction;
using FiapEcommerce.Domain.PurchaseTransaction.ChainOfResponsibility;
using FiapEcommerce.Domain.PurchaseTransaction.Composite;
using FiapEcommerce.Domain.PurchaseTransaction.DomainEvents;
using FiapEcommerce.Domain.PurchaseTransaction.DomainEvents.Subscribers;
using FiapEcommerce.Domain.PurchaseTransaction.Financial;
using FiapEcommerce.Domain.PurchaseTransaction.Strategy;
using FiapEcommerce.Infra.Data;
using FiapEcommerce.Infra.Financial;
using FiapEcommerce.Infra.Financial.PagSeguro;
using FiapEcommerce.Infra.Financial.PayPal;
using FiapEcommerce.Infra.Sap;

namespace FiapEcommerce.Api.Configuration;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDomainRepositories(this IServiceCollection services)
    {
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        return services;
    }

    public static IServiceCollection AddOrderDomainServices(this IServiceCollection services)
    {
        services.AddScoped<OrderServiceBasic>();
        services.AddScoped<OrderServiceWithBehavioralPatterns>();
        services.AddScoped<IOrderService>(sp => sp.GetRequiredService<OrderServiceWithBehavioralPatterns>());
        services.AddScoped<IOrderBuilder, OrderBuilder>();
        services.AddScoped<IDiscountConfiguration, DiscountConfiguration>();
        services.AddScoped<IAccountingService, SapAccountingFacade>();
        return services;
    }

    public static IServiceCollection AddPaymentInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IPaymentGatewayFactory, PaymentGatewayFactory>();
        services.AddScoped<PagSeguroService>();
        services.AddScoped<PayPalApi>();
        services.AddScoped<ISapBapiService, SapBapiService>();
        services.AddScoped<ISapIdocService, SapIdocService>();
        services.AddScoped<ISapRfcService, SapRfcService>();
        return services;
    }

    public static IServiceCollection AddBehavioralPatterns(this IServiceCollection services)
    {
        services.AddScoped<IPaymentProcessingStrategy, SinglePaymentStrategy>();
        services.AddScoped<IPaymentProcessingStrategy, InstallmentPaymentStrategy>();
        services.AddScoped<PaymentStrategyContext>();
        services.AddScoped<OrderProcessingChain>();

        services.AddSingleton<IOrderEventPublisher, OrderEventPublisher>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IAuditLogService, AuditLogService>();
        services.AddScoped<EmailNotificationSubscriber>();
        services.AddScoped<InventoryUpdateSubscriber>();
        services.AddScoped<AuditLogSubscriber>();

        return services;
    }

    public static WebApplication UseDomainEventSubscribers(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var publisher = scope.ServiceProvider.GetRequiredService<IOrderEventPublisher>();
        var emailSubscriber = scope.ServiceProvider.GetRequiredService<EmailNotificationSubscriber>();
        var inventorySubscriber = scope.ServiceProvider.GetRequiredService<InventoryUpdateSubscriber>();
        var auditSubscriber = scope.ServiceProvider.GetRequiredService<AuditLogSubscriber>();

        publisher.Subscribe(emailSubscriber);
        publisher.Subscribe(inventorySubscriber);
        publisher.Subscribe(auditSubscriber);

        return app;
    }
}
