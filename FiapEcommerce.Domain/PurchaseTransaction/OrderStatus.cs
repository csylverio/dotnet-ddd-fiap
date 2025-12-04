namespace FiapEcommerce.Domain.PurchaseTransaction;

public enum OrderStatus
{
    // Status iniciais
    Draft = 0,               // Pedido criado (não finalizado)
    Pending = 1,             // Pedido pendente (não finalizado)
    
    // Fluxo de pagamento
    AwaitingPayment = 2,     // Aguardando confirmação de pagamento (ex: boleto)
    PaymentApproved = 3,     // Pagamento aprovado
    PaymentFailed = 4,       // Pagamento recusado ou falhou
    PaymentRefunded = 5,     // Pagamento estornado/reembolsado
    PaymentError = 6,        // Erro no processamento de pagamento
    
    // Fluxo de preparação/entrega
    Processing = 7,          // Em preparação (separação de estoque)
    Shipped = 8,             // Enviado para transporte
    InTransit = 9,           // Em trânsito (rastreio)
    OutForDelivery = 10,      // Saiu para entrega
    
    // Finalização
    Delivered = 11,          // Entregue ao cliente
    Returned = 12,           // Devolvido pelo cliente
    Canceled = 13            // Cancelado (antes da entrega)
}
