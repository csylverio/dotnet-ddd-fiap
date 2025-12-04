using FiapEcommerce.Domain.CustomerRelationshipManagement;

namespace FiapEcommerce.Domain.PurchaseTransaction.ChainOfResponsibility;

public class BusinessRulesHandler : BaseOrderProcessingHandler
{
    private readonly ICustomerRepository _customerRepository;
    private const decimal MaxOrderValue = 10000m;
    private const decimal MinOrderValue = 1m;
    private const int MaxItemsPerOrder = 50;

    public BusinessRulesHandler(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    protected override Task<OrderProcessingResult> ProcessAsync(OrderProcessingContext context)
    {
        var errors = new List<string>();
        var warnings = new List<string>();

        // Verificar valor mínimo e máximo do pedido
        if (context.Order.TotalAmount < MinOrderValue)
        {
            errors.Add($"Valor mínimo do pedido é R$ {MinOrderValue:F2}");
        }

        if (context.Order.TotalAmount > MaxOrderValue)
        {
            errors.Add($"Valor máximo do pedido é R$ {MaxOrderValue:F2}");
        }

        // Verificar número máximo de itens
        if (context.Order.Items.Count > MaxItemsPerOrder)
        {
            errors.Add($"Número máximo de itens por pedido é {MaxItemsPerOrder}");
        }

        // Verificar regras específicas do cliente
        var customer = _customerRepository.GetById(context.Order.CustomerId);
        if (customer != null)
        {
            // Verificar se é primeira compra para descontos especiais
            var isFirstPurchase = IsFirstPurchase(customer.Id);
            if (isFirstPurchase && context.Order.TotalAmount > 5000m)
            {
                warnings.Add("Primeira compra com valor alto - verificar se cliente é legítimo");
            }

            // Verificar aniversário para descontos especiais
            if (IsBirthdayMonth(customer.BirthDate))
            {
                context.AdditionalData["IsBirthdayMonth"] = true;
                context.AddLog("Cliente está no mês de aniversário - elegível para desconto especial");
            }

            // Verificar histórico de compras
            var customerOrderHistory = GetCustomerOrderHistory(customer.Id);
            if (customerOrderHistory.AverageOrderValue > 0 && 
                context.Order.TotalAmount > customerOrderHistory.AverageOrderValue * 3)
            {
                warnings.Add($"Pedido com valor muito acima da média do cliente (média: R$ {customerOrderHistory.AverageOrderValue:F2})");
            }
        }

        // Verificar regras de horário (exemplo: pedidos grandes fora do horário comercial)
        var currentHour = DateTime.Now.Hour;
        if (context.Order.TotalAmount > 2000m && (currentHour < 8 || currentHour > 18))
        {
            warnings.Add("Pedido de alto valor fora do horário comercial");
        }

        // Verificar se há produtos incompatíveis no mesmo pedido
        var productCategories = context.Order.Items.Select(i => GetProductCategory(i.ProductId)).Distinct().ToList();
        if (HasIncompatibleProducts(productCategories))
        {
            warnings.Add("Pedido contém produtos de categorias incompatíveis");
        }

        if (errors.Any())
        {
            return Task.FromResult(OrderProcessingResult.FailureResult("Regras de negócio violadas", errors));
        }

        var result = OrderProcessingResult.SuccessResult("Regras de negócio verificadas com sucesso");
        result.Warnings = warnings;
        
        return Task.FromResult(result);
    }

    private bool IsFirstPurchase(int customerId)
    {
        // Simulação - em um sistema real consultaria o histórico de pedidos
        return new Random(customerId).Next(1, 10) == 1; // 10% de chance de ser primeira compra
    }

    private bool IsBirthdayMonth(DateTime birthDate)
    {
        return birthDate.Month == DateTime.Now.Month;
    }

    private (int OrderCount, decimal AverageOrderValue) GetCustomerOrderHistory(int customerId)
    {
        // Simulação de histórico do cliente
        var random = new Random(customerId);
        var orderCount = random.Next(1, 20);
        var averageOrderValue = random.Next(100, 1000);
        
        return (orderCount, averageOrderValue);
    }

    private string GetProductCategory(int productId)
    {
        // Simulação de categoria do produto
        var categories = new[] { "Eletrônicos", "Roupas", "Livros", "Casa", "Esportes" };
        return categories[productId % categories.Length];
    }

    private bool HasIncompatibleProducts(List<string> categories)
    {
        // Exemplo: eletrônicos e livros são incompatíveis para promoções
        return categories.Contains("Eletrônicos") && categories.Contains("Livros");
    }
}
