using System;

namespace FiapEcommerce.Domain.PurchaseTransaction.Composite.Rules;

public class PercentageDiscountLeaf : IDiscountRuleComponent
{
    private readonly decimal _percentage;

    public PercentageDiscountLeaf(decimal percentage)
    {
        _percentage = percentage;
    }

    public decimal CalculateDiscount(Order order)
    {
        return order.TotalAmount * _percentage / 100;
    }
}
