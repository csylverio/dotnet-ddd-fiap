using System;
using FiapEcommerce.Domain.InventoryManagement;

namespace FiapEcommerce.Infra.Data;

public class ProductRepository : IProductRepository
{
    public Product GetById(int productId)
    {
        Console.WriteLine("ProductRepository.GetById");
        return OrderFakerGenerator.ProductFaker;
    }
}
