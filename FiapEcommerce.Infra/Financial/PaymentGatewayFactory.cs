using Microsoft.Extensions.DependencyInjection;
using FiapEcommerce.Domain.PurchaseTransaction.Financial;
using FiapEcommerce.Infra.Financial.PagSeguro;
using FiapEcommerce.Infra.Financial.PayPal;

namespace FiapEcommerce.Infra.Financial;

public class PaymentGatewayFactory : IPaymentGatewayFactory
{
    private readonly IServiceProvider _provider;

    public PaymentGatewayFactory(IServiceProvider provider)
    {
        _provider = provider;
    }

    public IPaymentGateway Create(int paymentMethodId)
    {
        IPaymentGateway paymentGateway = paymentMethodId switch
        {
            1 => new PagSeguroAdapter(_provider.GetRequiredService<PagSeguroService>()),
            2 => new PayPalAdapter(_provider.GetRequiredService<PayPalApi>()),
            _ => throw new InvalidOperationException("Provedor de pagamento inválido"),
        };
        return paymentGateway;
    }
}
