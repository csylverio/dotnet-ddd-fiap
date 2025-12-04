using FiapEcommerce.Domain.PurchaseTransaction.Financial;

namespace FiapEcommerce.Domain.PurchaseTransaction.Strategy;

/// <summary>
/// Estratégia responsável por pagamentos à vista no contexto de pedidos.
/// Representa o papel ConcreteStrategy do padrão Strategy.
/// É orquestrada pelo <see cref="PaymentStrategyContext"/> e notifica o serviço de pedidos sobre o próximo status.
/// </summary>
public class SinglePaymentStrategy : IPaymentProcessingStrategy
{
    private readonly IPaymentGatewayFactory _paymentGatewayFactory;

    public SinglePaymentStrategy(IPaymentGatewayFactory paymentGatewayFactory)
    {
        _paymentGatewayFactory = paymentGatewayFactory;
    }

    public string StrategyName => "Pagamento à Vista";

    #region Métodos do padrão Strategy

    public bool CanHandle(PaymentType paymentType)
    {
        return paymentType == PaymentType.SinglePayment;
    }

    public async Task<PaymentResult> ProcessPaymentAsync(Order order, Payment payment)
    {
        var validationResult = await ValidatePaymentAsync(payment);
        if (!validationResult.Success)
        {
            return validationResult;
        }

        try
        {
            var gateway = _paymentGatewayFactory.Create(payment.PaymentMethodId);
            var gatewayResponse = await gateway.ProcessPaymentAsync(payment);

            if (gatewayResponse.IsSuccess)
            {
                payment.Status = PaymentStatus.Approved;
                payment.ProcessedAt = DateTime.UtcNow;
                payment.TransactionId = gatewayResponse.GatewayTransactionId;

                return new PaymentResult
                {
                    Success = true,
                    Status = "Aprovado",
                    TransactionId = gatewayResponse.GatewayTransactionId,
                    Message = "Pagamento processado com sucesso",
                    NextStep = OrderStatus.PaymentApproved
                };
            }
            else
            {
                payment.Status = PaymentStatus.Declined;
                return new PaymentResult
                {
                    Success = false,
                    Status = "Rejeitado",
                    ErrorMessage = gatewayResponse.ErrorMessage,
                    NextStep = OrderStatus.PaymentError
                };
            }
        }
        catch (Exception ex)
        {
            payment.Status = PaymentStatus.Error;
            return new PaymentResult
            {
                Success = false,
                Status = "Erro",
                ErrorMessage = "Erro interno no processamento do pagamento",
                NextStep = OrderStatus.PaymentError
            };
        }
    }

    public async Task<PaymentResult> ValidatePaymentAsync(Payment payment)
    {
        var errors = new List<string>();

        if (payment.Amount <= 0)
            errors.Add("Valor do pagamento deve ser maior que zero");

        if (string.IsNullOrEmpty(payment.CardNumber))
            errors.Add("Número do cartão é obrigatório");

        if (payment.PaymentMethodId <= 0)
            errors.Add("Método de pagamento inválido");

        if (errors.Any())
        {
            return new PaymentResult
            {
                Success = false,
                Status = "Validação Falhou",
                ErrorMessage = string.Join("; ", errors),
                NextStep = OrderStatus.PaymentError
            };
        }

        return new PaymentResult { Success = true };
    }

    #endregion
}
