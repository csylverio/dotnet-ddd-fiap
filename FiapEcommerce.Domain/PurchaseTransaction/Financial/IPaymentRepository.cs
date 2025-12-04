using System;

namespace FiapEcommerce.Domain.PurchaseTransaction.Financial;

public interface IPaymentRepository
{
    void Add(Payment payment);
}
