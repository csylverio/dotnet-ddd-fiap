using FiapEcommerce.Domain.CustomerRelationshipManagement;
using FiapEcommerce.Domain.InventoryManagement;

namespace FiapEcommerce.Domain.PurchaseTransaction.ChainOfResponsibility;

/// <summary>
/// Monta e executa a cadeia de responsabilidade para validação de pedidos.
/// Atua como o objeto "Chain" que conecta Handlers em uma sequência didática.
/// Colabora com o serviço de pedido para garantir pré-condições antes de usar os demais padrões.
/// </summary>
public class OrderProcessingChain
{
    private readonly IOrderProcessingHandler _chain;

    public OrderProcessingChain(
        ICustomerRepository customerRepository,
        IProductRepository productRepository)
    {
        #region Dependências da cadeia

        // Construir a cadeia de responsabilidade
        var orderValidation = new OrderValidationHandler();
        var inventoryCheck = new InventoryCheckHandler(productRepository);
        var businessRules = new BusinessRulesHandler(customerRepository);

        // Configurar a sequência da cadeia
        orderValidation
            .SetNext(inventoryCheck)
            .SetNext(businessRules);

        #endregion

        _chain = orderValidation;
    }

    #region Métodos públicos principais

    public async Task<OrderProcessingResult> ProcessOrderAsync(Order order, string action = "validate")
    {
        var context = new OrderProcessingContext(order, action);
        context.AddLog($"Iniciando processamento do pedido {order.Id} - Ação: {action}");

        try
        {
            var result = await _chain.HandleAsync(context);
            
            // Adicionar log de processamento ao resultado
            result.Data["ProcessingLog"] = context.ProcessingLog;
            
            if (result.Success)
            {
                context.AddLog("Processamento concluído com sucesso");
            }
            else
            {
                context.AddLog($"Processamento falhou: {result.Message}");
            }

            return result;
        }
        catch (Exception ex)
        {
            context.AddLog($"Erro durante processamento: {ex.Message}");
            
            var errorResult = OrderProcessingResult.FailureResult($"Erro interno durante processamento: {ex.Message}");
            errorResult.Data["ProcessingLog"] = context.ProcessingLog;
            
            return errorResult;
        }
    }

    // Método para criar cadeias customizadas para diferentes cenários
    public static OrderProcessingChain CreateCustomChain(params IOrderProcessingHandler[] handlers)
    {
        if (!handlers.Any())
            throw new ArgumentException("Pelo menos um handler deve ser fornecido");

        // Conectar os handlers em sequência
        for (int i = 0; i < handlers.Length - 1; i++)
        {
            handlers[i].SetNext(handlers[i + 1]);
        }

        return new OrderProcessingChain(handlers[0]);
    }

    #endregion

    private OrderProcessingChain(IOrderProcessingHandler chain)
    {
        _chain = chain;
    }
}
