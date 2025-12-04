using System;
using System.Threading.Tasks;
using FiapEcommerce.Domain.PurchaseTransaction.Financial;

namespace FiapEcommerce.Infra.Financial.PayPal;

public class PayPalAdapter : IPaymentGateway
{
    private readonly PayPalApi _payPalApi;

    public PayPalAdapter(PayPalApi payPalApi)
    {
        _payPalApi = payPalApi;
    }


    public PaymentGatewayResponse ProcessPayment(decimal amount)
    {
        var token = _payPalApi.Login("user1", "123456");    // valores poderiam vir do properties?
        var transaction = _payPalApi.OpenTransaction(token.ToString());
        transaction.Value = double.Parse(amount.ToString());
        var response = _payPalApi.SendPayment(transaction);

        return new PaymentGatewayResponse
        {
            GatewayTransactionId = response.Id.ToString(),
            IsSuccess = response.Status == "OK",
            ErrorMessage = response.Status == "OK" ? "" : "Error processing payment",
            ProcessedAmount = decimal.Parse(response.Value.ToString()),
            RawResponse = response.Response,
            ReceiptUrl = response.Url
        };
    }

    public Task<PaymentGatewayResponse> ProcessPaymentAsync(Payment payment)
    {
        return Task.FromResult(ProcessPayment(payment.Amount));
    }
}
