using System.Linq;

namespace FiapEcommerce.Domain.PurchaseTransaction.ChainOfResponsibility;

public class OrderValidationHandler : BaseOrderProcessingHandler
{
    protected override async Task<OrderProcessingResult> ProcessAsync(OrderProcessingContext context)
    {
        var errors = new List<string>();

        // Validação básica do pedido
        if (context.Order == null)
        {
            errors.Add("Pedido é obrigatório");
        }
        else
        {
            // Validar cliente
            if (context.Order.CustomerId <= 0)
                errors.Add("Cliente é obrigatório");

            // Validar itens
            if (!context.Order.Items.Any())
                errors.Add("Pedido deve ter pelo menos um item");

            foreach (var item in context.Order.Items)
            {
                if (item.Quantity <= 0)
                    errors.Add($"Quantidade do item {item.ProductId} deve ser maior que zero");

                if (item.UnitPrice <= 0)
                    errors.Add($"Preço unitário do item {item.ProductId} deve ser maior que zero");
            }

            // Validar valores
            if (context.Order.TotalAmount <= 0)
                errors.Add("Valor total do pedido deve ser maior que zero");

            // Validar desconto absoluto
            var grossTotal = context.Order.Items.Sum(i => i.TotalPrice);
            if (context.Order.Discount < 0 || context.Order.Discount > grossTotal)
                errors.Add("Desconto deve estar entre 0 e o valor total dos itens");
        }

        if (errors.Any())
        {
            return OrderProcessingResult.FailureResult("Validação do pedido falhou", errors);
        }

        return OrderProcessingResult.SuccessResult("Pedido validado com sucesso");
    }
}
