using System;

namespace FiapEcommerce.Domain.PurchaseTransaction.Composite.Rules;

public class VolumeDiscountLeaf : IDiscountRuleComponent
{
    public decimal CalculateDiscount(Order order)
    {
        if (order.Items.Count > 10)
            return order.TotalAmount * 0.05m; // 5% adicional para grandes pedidos (mais que 10 itens)
        return 0;
    }
}
