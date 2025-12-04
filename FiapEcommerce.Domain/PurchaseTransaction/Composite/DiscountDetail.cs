using System;

namespace FiapEcommerce.Domain.PurchaseTransaction.Composite;

public class DiscountDetail
{
    public decimal BaseDiscount { get; set; }
    public decimal PromotionalDiscount { get; set; }
    public decimal FinalDiscount { get; set; }
}
