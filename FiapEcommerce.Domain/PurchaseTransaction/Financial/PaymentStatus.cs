namespace FiapEcommerce.Domain.PurchaseTransaction.Financial;

public enum PaymentStatus
{
    Pending = 0,       // Aguardando processamento
    Approved = 1,      // Pagamento aprovado
    Declined = 2,      // Pagamento recusado
    Refunded = 3,      // Pagamento reembolsado
    Error = 4          // Erro no processamento
}
