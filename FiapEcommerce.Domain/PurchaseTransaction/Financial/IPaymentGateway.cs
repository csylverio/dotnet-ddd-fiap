using System;
using System.Threading.Tasks;

namespace FiapEcommerce.Domain.PurchaseTransaction.Financial;

public interface IPaymentGateway
{
    PaymentGatewayResponse ProcessPayment(decimal amount);
    Task<PaymentGatewayResponse> ProcessPaymentAsync(Payment payment);
}
