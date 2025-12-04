using System;

namespace FiapEcommerce.Domain.PurchaseTransaction.Financial;

public class PaymentGatewayResponse
{
    public string GatewayTransactionId { get; set; }   // ID específico do gateway
    
    // Status e resultado
    public bool IsSuccess { get; set; }               // Indica se foi bem-sucedido
   
    public decimal ProcessedAmount { get; set; }      // Valor processado
    
    // Mensagens e erros
    public string ErrorMessage { get; set; }          // Descrição do erro (se houver)
    public string ErrorCode { get; set; }             // Código de erro do gateway
    
    // Dados adicionais
    public string RawResponse { get; set; }           // Resposta bruta do gateway (JSON, XML)
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
    
    // URLs e recursos adicionais
    public string ReceiptUrl { get; set; }            // URL do comprovante
}
