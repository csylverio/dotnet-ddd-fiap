using System;
using System.Linq;
using FiapEcommerce.Domain.CustomerRelationshipManagement;
using FiapEcommerce.Domain.InventoryManagement;
using FiapEcommerce.Domain.PurchaseTransaction;
using FiapEcommerce.Domain.PurchaseTransaction.Composite;
using FiapEcommerce.Domain.PurchaseTransaction.Composite.Rules;
using Xunit;

namespace FiapEcommerce.Tests.Composite;

public class CompositeDiscountTests
{
    [Fact]
    public void Deve_Compor_Regras_Basicas_E_Promocionais()
    {
        // Arrange
        var order = CreateSampleOrder();
        var currentDate = new DateTime(2024, 11, 25); // Força Black Friday e mês de aniversário

        var baseComposite = new BaseDiscountsComposite();
        baseComposite.AddRule(new PercentageDiscountLeaf(10));

        var promotionalComposite = new PromotionalDiscountsComposite(order, currentDate);
        promotionalComposite.AddTemporaryPromotion(new CouponDiscountLeaf("DESC20"));

        // Act
        var baseDiscount = baseComposite.CalculateDiscount(order);
        var promotionalDiscount = promotionalComposite.CalculateDiscount(order);

        // Assert
        Assert.Equal(230m, baseDiscount); // 50 + 60 (5%) + 120 (10%)
        Assert.Equal(520m, promotionalDiscount); // 180 (Black Friday) + 100 (aniversário) + 240 (cupom)
        Assert.Equal(750m, baseDiscount + promotionalDiscount);
    }

    private static Order CreateSampleOrder()
    {
        var customer = new Customer
        {
            Id = 1,
            IsFirstPurchase = true,
            BirthDate = new DateTime(1990, 11, 1)
        };

        var product = new Product
        {
            Id = 10,
            Name = "Notebook",
            SalePrice = 100m
        };

        var items = Enumerable.Range(0, 12)
            .Select(_ => new Item(product, 1))
            .ToList();

        return new Order(customer, 0, items)
        {
            CustomerId = customer.Id,
            Discount = 0
        };
    }
}
