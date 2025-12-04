using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FiapEcommerce.Domain.CustomerRelationshipManagement;
using FiapEcommerce.Domain.InventoryManagement;
using FiapEcommerce.Domain.PurchaseTransaction;
using FiapEcommerce.Domain.PurchaseTransaction.ChainOfResponsibility;
using Xunit;

namespace FiapEcommerce.Tests.Chain;

public class OrderProcessingChainTests
{
    [Fact]
    public async Task Deve_Percorrer_Toda_Cadeia_de_Responsabilidade()
    {
        var chain = new OrderProcessingChain(new FakeCustomerRepository(), new FakeProductRepository());
        var order = BuildValidOrder();

        var result = await chain.ProcessOrderAsync(order, "finalize");

        Assert.True(result.Success);
        var log = Assert.IsType<List<string>>(result.Data["ProcessingLog"]);
        Assert.Contains(log, entry => entry.Contains(nameof(OrderValidationHandler)));
        Assert.Contains(log, entry => entry.Contains(nameof(InventoryCheckHandler)));
        Assert.Contains(log, entry => entry.Contains(nameof(BusinessRulesHandler)));
    }

    private static Order BuildValidOrder()
    {
        var product = new Product { Id = 5, Name = "Mouse", SalePrice = 200m, Active = true };
        var items = new List<Item> { new(product, 2) };

        return new Order(new Customer { Id = 1, IsFirstPurchase = false }, 0, items)
        {
            CustomerId = 1
        };
    }

    private sealed class FakeCustomerRepository : ICustomerRepository
    {
        public Customer GetById(int customerId)
        {
            return new Customer
            {
                Id = customerId,
                Name = "Cliente Teste",
                BirthDate = DateTime.Today
            };
        }
    }

    private sealed class FakeProductRepository : IProductRepository
    {
        public Product GetById(int productId)
        {
            return new Product
            {
                Id = productId,
                Name = "Produto Teste",
                SalePrice = 200m,
                Active = true
            };
        }
    }
}
