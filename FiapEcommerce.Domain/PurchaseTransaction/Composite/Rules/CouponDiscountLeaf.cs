using System;

namespace FiapEcommerce.Domain.PurchaseTransaction.Composite.Rules;

public class CouponDiscountLeaf : IDiscountRuleComponent
{
    private readonly string _couponCode;
    
    public CouponDiscountLeaf(string couponCode)
    {
        _couponCode = couponCode;
    }

    public decimal CalculateDiscount(Order order)
    {
        // TODO: Cupons válidos deveria ser obtidos por meio de repositório através de injetado dependencia 
        // Lógica para validar o cupom e calcular desconto
        if (_couponCode == "DESC20")
            return order.TotalAmount * 0.20m;
        
        return 0;
    }
}
