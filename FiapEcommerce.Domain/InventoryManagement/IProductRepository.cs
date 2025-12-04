using System;

namespace FiapEcommerce.Domain.InventoryManagement;

public interface IProductRepository
{
    Product GetById(int productId);
}
