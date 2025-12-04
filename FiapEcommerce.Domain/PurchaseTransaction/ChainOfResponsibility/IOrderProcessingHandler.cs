namespace FiapEcommerce.Domain.PurchaseTransaction.ChainOfResponsibility;

public interface IOrderProcessingHandler
{
    IOrderProcessingHandler SetNext(IOrderProcessingHandler handler);
    Task<OrderProcessingResult> HandleAsync(OrderProcessingContext context);
}

public class OrderProcessingContext
{
    public Order Order { get; set; }
    public string Action { get; set; }
    public Dictionary<string, object> AdditionalData { get; set; } = new();
    public List<string> ProcessingLog { get; set; } = new();
    
    public OrderProcessingContext(Order order, string action)
    {
        Order = order;
        Action = action;
    }

    public void AddLog(string message)
    {
        ProcessingLog.Add($"{DateTime.UtcNow:HH:mm:ss} - {message}");
    }
}

public class OrderProcessingResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public Dictionary<string, object> Data { get; set; } = new();

    public static OrderProcessingResult SuccessResult(string message = "Processamento concluído com sucesso")
    {
        return new OrderProcessingResult { Success = true, Message = message };
    }

    public static OrderProcessingResult FailureResult(string message, List<string>? errors = null)
    {
        return new OrderProcessingResult 
        { 
            Success = false, 
            Message = message, 
            Errors = errors ?? new List<string>() 
        };
    }
}