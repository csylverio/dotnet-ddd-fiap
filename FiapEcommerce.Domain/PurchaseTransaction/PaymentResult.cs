using System;

namespace FiapEcommerce.Domain.PurchaseTransaction;

public class PaymentResult
{
    public bool Success { get; set; }
    public int PaymentId { get; set; }           // ID do registro no banco
    public string TransactionId { get; set; } = string.Empty;     // ID do gateway
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; }
    public int PaymentMethod { get; set; }
    public string Status { get; set; } = string.Empty;           // Status descritivo
    public string Message { get; set; } = string.Empty;          // Mensagem amigável
    public string ErrorMessage { get; set; } = string.Empty;
    public OrderStatus? NextStep { get; set; }     // Próximo status do pedido
}
