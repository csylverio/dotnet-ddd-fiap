using System;

namespace FiapEcommerce.Domain.PurchaseTransaction.Composite;

public interface IDiscountConfiguration
{
    bool ApplyPercentageDiscount { get; }
    decimal Percentage { get; }
    bool ApplyFixedDiscount { get; }
    decimal FixedAmount { get; }
}
