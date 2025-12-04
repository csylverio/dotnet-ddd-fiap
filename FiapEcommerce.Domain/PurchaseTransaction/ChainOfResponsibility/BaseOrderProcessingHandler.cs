namespace FiapEcommerce.Domain.PurchaseTransaction.ChainOfResponsibility;

public abstract class BaseOrderProcessingHandler : IOrderProcessingHandler
{
    private IOrderProcessingHandler? _nextHandler;

    public IOrderProcessingHandler SetNext(IOrderProcessingHandler handler)
    {
        _nextHandler = handler;
        return handler;
    }

    public virtual async Task<OrderProcessingResult> HandleAsync(OrderProcessingContext context)
    {
        var result = await ProcessAsync(context);
        
        if (!result.Success)
        {
            context.AddLog($"Falha em {GetType().Name}: {result.Message}");
            return result;
        }

        context.AddLog($"Sucesso em {GetType().Name}: {result.Message}");

        if (_nextHandler != null)
        {
            return await _nextHandler.HandleAsync(context);
        }

        return result;
    }

    protected abstract Task<OrderProcessingResult> ProcessAsync(OrderProcessingContext context);
}