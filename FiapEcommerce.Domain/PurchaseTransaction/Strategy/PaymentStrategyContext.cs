using FiapEcommerce.Domain.PurchaseTransaction.Financial;

namespace FiapEcommerce.Domain.PurchaseTransaction.Strategy;

public class PaymentStrategyContext
{
    private readonly IEnumerable<IPaymentProcessingStrategy> _strategies;

    public PaymentStrategyContext(IEnumerable<IPaymentProcessingStrategy> strategies)
    {
        _strategies = strategies;
    }

    public async Task<PaymentResult> ProcessPaymentAsync(Order order, Payment payment, PaymentType paymentType)
    {
        var strategy = GetStrategy(paymentType);
        
        if (strategy == null)
        {
            return new PaymentResult
            {
                Success = false,
                Status = "Erro",
                ErrorMessage = $"Estratégia de pagamento não encontrada para o tipo: {paymentType}",
                NextStep = OrderStatus.PaymentError
            };
        }

        return await strategy.ProcessPaymentAsync(order, payment);
    }

    public IPaymentProcessingStrategy? GetStrategy(PaymentType paymentType)
    {
        return _strategies.FirstOrDefault(s => s.CanHandle(paymentType));
    }

    public List<string> GetAvailableStrategies()
    {
        return _strategies.Select(s => s.StrategyName).ToList();
    }

    public async Task<PaymentResult> ValidatePaymentAsync(Payment payment, PaymentType paymentType)
    {
        var strategy = GetStrategy(paymentType);
        
        if (strategy == null)
        {
            return new PaymentResult
            {
                Success = false,
                Status = "Erro",
                ErrorMessage = $"Estratégia de validação não encontrada para o tipo: {paymentType}",
                NextStep = OrderStatus.PaymentError
            };
        }

        return await strategy.ValidatePaymentAsync(payment);
    }
}
