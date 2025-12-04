using System;

namespace FiapEcommerce.Domain.PurchaseTransaction;

public interface IOrderBuilder
{
    IOrderBuilder SetCustomerId(int customerId);
    IOrderBuilder SetPaymentMethod(int? paymentMethodId);
    IOrderBuilder SetShippingMethod(int? shippingMethodId);
    IOrderBuilder SetDiscount(decimal? discount);
    IOrderBuilder AddItem(int productId, int quantity);
    Order Build();
}
