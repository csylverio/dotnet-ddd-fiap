using System;
using FiapEcommerce.Domain.CustomerRelationshipManagement;
using FiapEcommerce.Domain.InventoryManagement;

namespace FiapEcommerce.Domain.PurchaseTransaction;

public class OrderBuilder : IOrderBuilder
{
    private Customer _customer { get; set; }
    private int? _paymentMethodId { get; set; }
    private int? _shippingMethodId { get; set; }
    private decimal _discount { get; set; }
    private List<Item> _items { get; set; } = new List<Item>();

    private readonly ICustomerRepository _customerRepository;
    private readonly IProductRepository _productRepository;

    public OrderBuilder(ICustomerRepository customerRepository, IProductRepository productRepository)
    {
        _customerRepository = customerRepository;
        _productRepository = productRepository;
    }

    public IOrderBuilder SetCustomerId(int customerId)
    {
        _customer = _customerRepository.GetById(customerId);
        if (_customer == null) throw new ArgumentException("Invalid customer");
        return this;
    }

    public IOrderBuilder SetPaymentMethod(int? paymentMethodId)
    {
        if (paymentMethodId.HasValue)
        {
            _paymentMethodId = paymentMethodId;
            //TODO: implementa regra de pagamento
            // Se 1 - Cartão de crétido
            // Se 2 - Cartão de Débito
            // Se 3 - PIX
        }
        return this;
    }

    public IOrderBuilder SetShippingMethod(int? shippingMethodId)
    {
        _shippingMethodId = shippingMethodId;
        //TODO: implementa regra de entrega
        // Regra Depende da localização do cliente para saber se cobra o a mais na entrega

        return this;
    }

    public IOrderBuilder SetDiscount(decimal? discount)
    {
        if (discount.HasValue)
            // TODO: regra pode variar com o método de pagamento (só pode 5% para cartão de crétido)
            _discount = discount.Value;
        return this;
    }

    public IOrderBuilder AddItem(int productId, int quantity)
    {
        Product product = _productRepository.GetById(productId);
        Item item = new(product, quantity);
        _items.Add(item);
        return this;
    }

    public Order Build()
    {
        return new Order(_customer, _discount, _items);
    }


}