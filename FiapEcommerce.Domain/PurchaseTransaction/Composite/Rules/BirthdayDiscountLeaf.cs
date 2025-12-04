using System;

namespace FiapEcommerce.Domain.PurchaseTransaction.Composite.Rules;

public class BirthdayDiscountLeaf : IDiscountRuleComponent
{
    public decimal CalculateDiscount(Order order)
    {
        return 100; // R$100 de desconto no mês de aniversário
    }
}
