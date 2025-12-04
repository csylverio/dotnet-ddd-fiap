using Bogus;
using FiapEcommerce.Domain.CustomerRelationshipManagement;
using FiapEcommerce.Domain.InventoryManagement;
using FiapEcommerce.Domain.PurchaseTransaction;
using FiapEcommerce.Domain.PurchaseTransaction.Financial;
namespace FiapEcommerce.Infra.Data;

internal static class OrderFakerGenerator
{
    internal static Faker<Customer> CustomerFaker => new Faker<Customer>()
           .RuleFor(c => c.Id, f => f.Random.Int(1, 1000))
           .RuleFor(c => c.Name, f => f.Name.FullName())
           .RuleFor(c => c.Email, f => f.Internet.Email())
           .RuleFor(c => c.BirthDate, f => f.Date.Past(30, DateTime.Today.AddYears(-18)))
           .RuleFor(c => c.IsActive, f => f.Random.Bool())
           .RuleFor(c => c.IsFirstPurchase, f => f.Random.Bool())
           .RuleFor(c => c.Cpf, f => f.Random.Replace("###.###.###-##"));

    internal static Faker<Product> ProductFaker => new Faker<Product>()
        .RuleFor(p => p.Id, f => f.Random.Int(1, 10000))
        .RuleFor(p => p.Name, f => f.Commerce.ProductName())
        .RuleFor(p => p.SalePrice, f => f.Finance.Amount(20, 200));

    internal static Faker<Item> ItemFaker => new Faker<Item>()
        .CustomInstantiator(f => new Item(ProductFaker.Generate(), f.Random.Int(1, 5)))
        .RuleFor(i => i.Id, f => f.Random.Int(1, 9999));

    internal static Faker<Payment> PaymentFaker => new Faker<Payment>()
        .RuleFor(p => p.Id, f => f.Random.Int(1000, 2000))
        .RuleFor(p => p.TransactionId, f => f.Random.Guid().ToString())
        .RuleFor(p => p.Amount, f => f.Finance.Amount(50, 500))
        .RuleFor(p => p.PaymentDate, f => f.Date.RecentOffset(days: 3).UtcDateTime)
        .RuleFor(p => p.PaymentMethodId, f => f.Random.Int(1, 3))
        .RuleFor(p => p.Status, f => PaymentStatus.Pending)
        .RuleFor(p => p.GatewayResponse, f => f.Lorem.Sentence())
        .RuleFor(p => p.LastFourDigits, f => f.Random.Replace("####"))
        .RuleFor(p => p.InstallmentCount, f => f.Random.Int(1, 12));

    internal static Order GenerateFakeOrder()
    {
        var customer = CustomerFaker.Generate();
        var items = ItemFaker.Generate(3);
        var order = new Order(customer, discount: 10, items)
        {
            Id = new Random().Next(1, 10000),
            CustomerId = customer.Id,
            Status = OrderStatus.Pending
        };

        var payments = PaymentFaker.Generate(2);
        foreach (var payment in payments)
        {
            payment.OrderId = order.Id;
            payment.Order = order;
        }

        order.Payments = payments;
        return order;
    }
}
