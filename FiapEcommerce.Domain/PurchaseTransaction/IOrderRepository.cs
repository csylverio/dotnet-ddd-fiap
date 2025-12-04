using System;

namespace FiapEcommerce.Domain.PurchaseTransaction;

public interface IOrderRepository
{
    void Add(Order order);
    Order GetById(int orderId);
    void Update(Order order);
}
