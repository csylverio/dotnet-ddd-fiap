using System;
using Bogus;
using FiapEcommerce.Domain.CustomerRelationshipManagement;
using FiapEcommerce.Domain.PurchaseTransaction;
using FiapEcommerce.Domain.PurchaseTransaction.Financial;

namespace FiapEcommerce.Infra.Data;

public class OrderRepository : IOrderRepository
{
    public Order GetById(int orderId)
    {
        Console.WriteLine("OrderRepository.GetById");
        return OrderFakerGenerator.GenerateFakeOrder();
    }

    public void Add(Order order)
    {
        Console.WriteLine("OrderRepository.Add");
    }

    public void Update(Order order)
    {
        Console.WriteLine("OrderRepository.Update");
    }
}
