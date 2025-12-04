using System;
using FiapEcommerce.Domain.PurchaseTransaction.Composite.Rules;

namespace FiapEcommerce.Domain.PurchaseTransaction.Composite;

/// <summary>
/// Composite responsável por agrupar regras de desconto sempre presentes no fluxo.
/// Representa o papel de "Composite" no padrão, somando o resultado de cada folha básica.
/// Trabalha em conjunto com os Leafs e com o serviço de pedido para construir o desconto final.
/// </summary>
public class BaseDiscountsComposite : IDiscountRuleComponent
{
    private readonly List<IDiscountRuleComponent> _baseRules = new();

    public BaseDiscountsComposite()
    {
        // Regras básicas que sempre se aplicam
        _baseRules.Add(new FirstPurchaseDiscountLeaf());
        _baseRules.Add(new VolumeDiscountLeaf());
    }

    #region Métodos do padrão Composite

    public void AddRule(IDiscountRuleComponent rule)
    {
        _baseRules.Add(rule);
    }

    public decimal CalculateDiscount(Order order)
    {
        return _baseRules.Sum(rule => rule.CalculateDiscount(order));
    }

    #endregion
}
