using System;
using FiapEcommerce.Domain.PurchaseTransaction.Composite.Rules;

namespace FiapEcommerce.Domain.PurchaseTransaction.Composite;

/// <summary>
/// Composite que modela as promoções condicionais do domínio (Black Friday, aniversário e cupons).
/// Também representa o papel "Composite" do padrão, mas com regras que podem ser ligadas e desligadas em tempo real.
/// Trabalha junto com o serviço de pedidos para reunir todas as folhas promocionais.
/// </summary>
public class PromotionalDiscountsComposite : IDiscountRuleComponent
{
    private readonly List<IDiscountRuleComponent> _promotionalRules = new();
    private readonly DateTime _currentDate;

    public PromotionalDiscountsComposite(Order order, DateTime? currentDate = null)
    {
        _currentDate = currentDate ?? DateTime.Now;
        
        // Regras promocionais condicionais
        if (IsBlackFridayPeriod())
        {
            _promotionalRules.Add(new BlackFridayDiscountLeaf());
        }
        
        if (IsBirthdayMonth(order.Customer.BirthDate))
        {
            _promotionalRules.Add(new BirthdayDiscountLeaf());
        }
    }

    private bool IsBlackFridayPeriod()
    {
        return _currentDate.Month == 11 && _currentDate.Day >= 20 && _currentDate.Day <= 30;
    }

    private bool IsBirthdayMonth(DateTime customerBirthDate)
    {
        return _currentDate.Month == customerBirthDate.Month;
    }

    #region Métodos do padrão Composite

    public void AddTemporaryPromotion(IDiscountRuleComponent promotion)
    {
        _promotionalRules.Add(promotion);
    }

    public decimal CalculateDiscount(Order order)
    {
        return _promotionalRules.Sum(rule => rule.CalculateDiscount(order));
    }

    #endregion
}
