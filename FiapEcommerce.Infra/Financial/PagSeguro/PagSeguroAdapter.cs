using System;
using System.Threading.Tasks;
using FiapEcommerce.Domain.PurchaseTransaction.Financial;

namespace FiapEcommerce.Infra.Financial.PagSeguro;

public class PagSeguroAdapter : IPaymentGateway
{
    private readonly PagSeguroService _pagSeguroService;

    public PagSeguroAdapter(PagSeguroService pagSeguroService)
    {
        _pagSeguroService = pagSeguroService;
    }

    public PaymentGatewayResponse ProcessPayment(decimal amount)
    {
        try
        {
            var filename = _pagSeguroService.GenerateFile();
            var fileSend = _pagSeguroService.SendFile(amount * 100); // valor deve ser em centavos

            return new PaymentGatewayResponse
            {
                GatewayTransactionId = Guid.NewGuid().ToString(), // Assuming PagSeguro returns a transaction ID
                IsSuccess = fileSend,
                ProcessedAmount = amount,
                RawResponse = filename
            };
        }
        catch (Exception ex)
        {
            return new PaymentGatewayResponse
            {
                GatewayTransactionId = Guid.NewGuid().ToString(), // Assuming PagSeguro returns a transaction ID
                IsSuccess = false,
                ProcessedAmount = amount,
                ErrorMessage = ex.Message
            };
        }
    }

    public Task<PaymentGatewayResponse> ProcessPaymentAsync(Payment payment)
    {
        return Task.FromResult(ProcessPayment(payment.Amount));
    }
}
