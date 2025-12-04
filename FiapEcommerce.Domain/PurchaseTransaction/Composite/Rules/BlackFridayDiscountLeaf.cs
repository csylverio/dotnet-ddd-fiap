using System;

namespace FiapEcommerce.Domain.PurchaseTransaction.Composite.Rules;

public class BlackFridayDiscountLeaf : IDiscountRuleComponent
{
    public decimal CalculateDiscount(Order order)
    {
        return order.TotalAmount * 0.15m; // 15% de desconto
    }
}
