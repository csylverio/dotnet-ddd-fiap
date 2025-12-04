using System;

namespace FiapEcommerce.Domain.PurchaseTransaction;

public interface IAccountingService
{
    AccountingResult RegisterSale(Order order);
}
