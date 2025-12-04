using System;
using FiapEcommerce.Domain.PurchaseTransaction.Financial;

namespace FiapEcommerce.Infra.Data;

public class PaymentRepository : IPaymentRepository
{
    public void Add(Payment payment)
    {
        Console.WriteLine("PaymentRepository.Add");
    }
}
