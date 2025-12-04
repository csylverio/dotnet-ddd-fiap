using System;

namespace FiapEcommerce.Domain.PurchaseTransaction.Composite;

/// <summary>
/// Interface que representa Component do padrão Composite
/// Descreve operações que são comuns tanto para elementos simples como para elementos complexos da árvore.
/// </summary>
public interface IDiscountRuleComponent
{
    decimal CalculateDiscount(Order order);
}
