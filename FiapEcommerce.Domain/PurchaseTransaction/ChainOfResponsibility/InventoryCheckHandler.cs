using FiapEcommerce.Domain.InventoryManagement;

namespace FiapEcommerce.Domain.PurchaseTransaction.ChainOfResponsibility;

public class InventoryCheckHandler : BaseOrderProcessingHandler
{
    private readonly IProductRepository _productRepository;

    public InventoryCheckHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    protected override Task<OrderProcessingResult> ProcessAsync(OrderProcessingContext context)
    {
        var errors = new List<string>();
        var warnings = new List<string>();

        foreach (var item in context.Order.Items)
        {
            var product = _productRepository.GetById(item.ProductId);
            
            if (product == null)
            {
                errors.Add($"Produto {item.ProductId} não encontrado");
                continue;
            }

            // Verificar se produto está ativo
            if (!product.Active)
            {
                errors.Add($"Produto {product.Name} não está mais disponível");
                continue;
            }

            // Verificar estoque (simulado - assumindo que Product tem uma propriedade Stock)
            var stockQuantity = GetProductStock(product);
            
            if (stockQuantity < item.Quantity)
            {
                if (stockQuantity == 0)
                {
                    errors.Add($"Produto {product.Name} está fora de estoque");
                }
                else
                {
                    errors.Add($"Produto {product.Name} - Estoque insuficiente. Disponível: {stockQuantity}, Solicitado: {item.Quantity}");
                }
            }
            else if (stockQuantity < item.Quantity * 2) // Aviso se estoque está baixo
            {
                warnings.Add($"Produto {product.Name} - Estoque baixo ({stockQuantity} unidades)");
            }

            // Verificar se preço está atualizado
            if (Math.Abs(product.Price - item.UnitPrice) > 0.01m)
            {
                warnings.Add($"Produto {product.Name} - Preço pode estar desatualizado. Atual: R$ {product.Price:F2}, Pedido: R$ {item.UnitPrice:F2}");
            }
        }

        if (errors.Any())
        {
            return Task.FromResult(OrderProcessingResult.FailureResult("Verificação de estoque falhou", errors));
        }

        var result = OrderProcessingResult.SuccessResult("Estoque verificado com sucesso");
        result.Warnings = warnings;
        
        return Task.FromResult(result);
    }

    private int GetProductStock(Product product)
    {
        // Simulação determinística apenas para demonstração/testes
        return (product.Id % 50) + 10;
    }
}
