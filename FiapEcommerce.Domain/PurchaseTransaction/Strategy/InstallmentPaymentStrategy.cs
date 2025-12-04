using FiapEcommerce.Domain.PurchaseTransaction.Financial;

namespace FiapEcommerce.Domain.PurchaseTransaction.Strategy;

/// <summary>
/// Estratégia de pagamentos parcelados que encapsula regras de juros e validações específicas.
/// Atua como ConcreteStrategy no padrão Strategy e mantém o serviço de pedidos isolado de variações.
/// Trabalha em conjunto com o <see cref="PaymentStrategyContext"/> para ser selecionada quando o tipo for parcelado.
/// </summary>
public class InstallmentPaymentStrategy : IPaymentProcessingStrategy
{
    private readonly IPaymentGatewayFactory _paymentGatewayFactory;
    private const int MaxInstallments = 12;
    private const decimal MinInstallmentAmount = 50m;

    public InstallmentPaymentStrategy(IPaymentGatewayFactory paymentGatewayFactory)
    {
        _paymentGatewayFactory = paymentGatewayFactory;
    }

    public string StrategyName => "Pagamento Parcelado";

    #region Métodos do padrão Strategy

    public bool CanHandle(PaymentType paymentType)
    {
        return paymentType == PaymentType.InstallmentPayment;
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
            // Calcular juros baseado no número de parcelas
            var finalAmount = CalculateInstallmentAmount(payment.Amount, payment.InstallmentCount);
            payment.Amount = finalAmount;

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
                    Message = $"Pagamento parcelado em {payment.InstallmentCount}x aprovado",
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
                ErrorMessage = "Erro interno no processamento do pagamento parcelado",
                NextStep = OrderStatus.PaymentError
            };
        }
    }

    public async Task<PaymentResult> ValidatePaymentAsync(Payment payment)
    {
        var errors = new List<string>();

        if (payment.Amount <= 0)
            errors.Add("Valor do pagamento deve ser maior que zero");

        if (payment.InstallmentCount <= 1 || payment.InstallmentCount > MaxInstallments)
            errors.Add($"Número de parcelas deve ser entre 2 e {MaxInstallments}");

        var installmentValue = payment.Amount / payment.InstallmentCount;
        if (installmentValue < MinInstallmentAmount)
            errors.Add($"Valor mínimo da parcela deve ser R$ {MinInstallmentAmount:F2}");

        if (string.IsNullOrEmpty(payment.CardNumber))
            errors.Add("Número do cartão é obrigatório para pagamento parcelado");

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

    private decimal CalculateInstallmentAmount(decimal originalAmount, int installmentCount)
    {
        // Tabela simplificada de juros por parcela
        var interestRates = new Dictionary<int, decimal>
        {
            { 2, 0.02m },  // 2% para 2x
            { 3, 0.03m },  // 3% para 3x
            { 4, 0.05m },  // 5% para 4x
            { 5, 0.07m },  // 7% para 5x
            { 6, 0.09m },  // 9% para 6x
            { 7, 0.11m },  // 11% para 7x
            { 8, 0.13m },  // 13% para 8x
            { 9, 0.15m },  // 15% para 9x
            { 10, 0.17m }, // 17% para 10x
            { 11, 0.19m }, // 19% para 11x
            { 12, 0.21m }  // 21% para 12x
        };

        var interestRate = interestRates.ContainsKey(installmentCount) ? interestRates[installmentCount] : 0.25m;
        return originalAmount * (1 + interestRate);
    }
}
