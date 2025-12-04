using FiapEcommerce.Domain.PurchaseTransaction.Financial;

namespace FiapEcommerce.Domain.PurchaseTransaction.Strategy;

/// <summary>
/// Define o contrato das estratégias de pagamento do domínio no padrão Strategy.
/// Permite plugar novos comportamentos de processamento sem alterar o serviço de pedidos.
/// Trabalha em conjunto com o <see cref="PaymentStrategyContext"/> para seleção dinâmica.
/// </summary>
public interface IPaymentProcessingStrategy
{
    string StrategyName { get; }
    bool CanHandle(PaymentType paymentType);
    Task<PaymentResult> ProcessPaymentAsync(Order order, Payment payment);
    Task<PaymentResult> ValidatePaymentAsync(Payment payment);
}

public enum PaymentType
{
    SinglePayment = 1,
    InstallmentPayment = 2,
    RecurringPayment = 3,
    CorporatePayment = 4
}
