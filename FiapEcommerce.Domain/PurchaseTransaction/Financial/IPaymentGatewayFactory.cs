using System;

namespace FiapEcommerce.Domain.PurchaseTransaction.Financial;

public interface IPaymentGatewayFactory
{
    IPaymentGateway Create(int paymentMethodId);
}
