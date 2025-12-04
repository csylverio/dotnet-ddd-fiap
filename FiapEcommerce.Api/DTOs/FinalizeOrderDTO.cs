using System;
using FiapEcommerce.Domain.PurchaseTransaction;

namespace FiapEcommerce.Api.DTOs;

public class FinalizeOrderDTO
{
    public int OrderId { get; set; }
    public string? CouponCode { get; set; }
}
