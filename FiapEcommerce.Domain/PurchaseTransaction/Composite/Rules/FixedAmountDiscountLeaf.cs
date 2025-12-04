using System;

namespace FiapEcommerce.Domain.PurchaseTransaction.Composite.Rules;

public class FixedAmountDiscountLeaf : IDiscountRuleComponent
{
    private readonly decimal _amount;

    public FixedAmountDiscountLeaf(decimal amount)
    {
        _amount = amount;
    }

    public decimal CalculateDiscount(Order order)
    {
        return _amount;
    }
}
