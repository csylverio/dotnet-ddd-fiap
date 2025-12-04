using System;
using FiapEcommerce.Domain.InventoryManagement;

namespace FiapEcommerce.Domain.PurchaseTransaction;

public class Item
{
    public Item()
    {
    }

    public Item(Product product, int quantity)
    {
        if (quantity <= 0) throw new ArgumentException("Quantity must be greater than zero");
        
        Product = product;
        ProductId = product?.Id ?? 0;
        Quantity = quantity;
        UnitPrice = product.SalePrice;
    }

    public int Id { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; } // TODO: substituir por Value Object Money com propriedade valor e moeda
    public Product Product { get; set; }
    public decimal TotalPrice => Quantity * UnitPrice;
}
