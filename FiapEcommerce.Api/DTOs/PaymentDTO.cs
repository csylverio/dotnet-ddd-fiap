using System;
using FiapEcommerce.Domain.PurchaseTransaction.Strategy;

namespace FiapEcommerce.Api.DTOs;

public class PaymentDTO
{
    public int OrderId { get; set; }
    public int PaymentMethodId { get; set; }
    public string? CardNumber { get; set; }      // Para cartões (opcional)
    public string? LastFourDigits { get; set; }   // Últimos 4 dígitos do cartão
    public string? CustomerId { get; set; }      // Identificador do comprador (opcional)
    public int Installments { get; set; } = 1;
    public PaymentType PaymentType { get; set; } = PaymentType.SinglePayment;
}
