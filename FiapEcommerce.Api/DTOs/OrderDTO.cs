using System;
using FiapEcommerce.Domain.PurchaseTransaction;

namespace FiapEcommerce.Api.DTOs;

public class OrderDTO
{
    public int CustomerId { get; set; }
    /// <summary>
    /// Método de pagamento
    /// </summary>
    public int? PaymentMethodId { get; set; }

    /// <summary>
    /// Método de entrega
    /// </summary>
    public int? ShippingMethodId { get; set; }
    public decimal Discount { get; set; } = 0;
    public List<OrderItemDTO> Items { get; set; } = new List<OrderItemDTO>();
}
