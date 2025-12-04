using System;

namespace FiapEcommerce.Domain.PurchaseTransaction.Financial;

public class Payment
{
    public Payment()
    {
        PaymentDate = DateTime.UtcNow;
        Status = PaymentStatus.Pending;
    }
    
    public int Id { get; set; }
    public string TransactionId { get; set; }          // ID único do gateway de pagamento
    public int OrderId { get; set; }                   // Pedido associado
    public Order Order { get; set; }                   // Navegação para Order
    public decimal Amount { get; set; }                // Valor pago
    public DateTime PaymentDate { get; set; }          // Data/hora do pagamento
    public int PaymentMethodId { get; set; }           // ID do método (cartão, PIX, etc.)
    public PaymentStatus Status { get; set; }          // Status do pagamento
    public string GatewayResponse { get; set; }        // Resposta bruta do gateway
    public DateTime? RefundDate { get; set; }          // Data de reembolso (se aplicável)
    public string ErrorMessage { get; set; }           // Mensagem de erro (se houver)
    public DateTime? ProcessedAt { get; set; }         // Data/hora do processamento

    // Dados adicionais para cartão (opcional)
    public string? CardNumber { get; set; }             // Para cartões (opcional)
    public string? LastFourDigits { get; set; }         // Últimos 4 dígitos do cartão (opcional)
    public int InstallmentCount { get; set; } = 1;     // Número de parcelas (padrão 1)
}
