using System;

namespace FiapEcommerce.Domain.PurchaseTransaction.Composite;

/// <summary>
/// Singleton Pattern gerenciada pelo framework
/// Representa configurações de desconto, configurações poderiam ser "setadas" a partir de um arquivo de configuração ou base de dados
/// </summary>
public class DiscountConfiguration : IDiscountConfiguration
{
    public bool ApplyPercentageDiscount { get; } = true;
    public decimal Percentage { get; } = 10;
    public bool ApplyFixedDiscount { get; } = false;
    public decimal FixedAmount { get; } = 5;
}
