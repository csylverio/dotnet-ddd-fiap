using System;

namespace FiapEcommerce.Domain.InventoryManagement;

/// <summary>
/// Representa uma instância de produtos de um estabelecimento
/// </summary>
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public decimal SalePrice { get; set; }
    public bool Active { get; set; }
}
