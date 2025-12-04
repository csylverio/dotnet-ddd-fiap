using System;

namespace FiapEcommerce.Domain.PurchaseTransaction.Composite.Rules;

public class FirstPurchaseDiscountLeaf : IDiscountRuleComponent
{
  public decimal CalculateDiscount(Order order)
    {
        // Verifica se é a primeira compra do cliente
        return order.Customer.IsFirstPurchase ? 50 : 0; // R$50 de desconto
    }
}
