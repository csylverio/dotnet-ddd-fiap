using System;

namespace FiapEcommerce.Api.DTOs;

public class OrderItemDTO
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}
