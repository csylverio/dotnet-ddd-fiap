namespace FiapEcommerce.Domain.PurchaseTransaction.DomainEvents.Subscribers;

/// <summary>
/// Subscriber responsável por transformar eventos de domínio em e-mails para o cliente.
/// Representa o Observer concreto dentro do padrão Observer.
/// É inscrito pelo <see cref="OrderEventPublisher"/> e depende de <see cref="IEmailService"/> para o envio.
/// </summary>
public class EmailNotificationSubscriber : IOrderEventSubscriber
{
    private readonly IEmailService _emailService;

    public EmailNotificationSubscriber(IEmailService emailService)
    {
        _emailService = emailService;
    }

    #region Métodos do Observer

    public async Task OnOrderCreatedAsync(OrderCreatedEvent orderEvent)
    {
        var subject = $"Pedido #{orderEvent.OrderId} - Confirmação de Pedido";
        var body = GenerateOrderCreatedEmailBody(orderEvent);
        
        await _emailService.SendEmailAsync(orderEvent.CustomerEmail, subject, body);
        Console.WriteLine($"📧 Email de confirmação enviado para {orderEvent.CustomerEmail}");
    }

    public async Task OnOrderStatusChangedAsync(OrderStatusChangedEvent orderEvent)
    {
        var statusMessages = new Dictionary<OrderStatus, string>
        {
            { OrderStatus.PaymentApproved, "Pagamento aprovado! Seu pedido está sendo processado." },
            { OrderStatus.Processing, "Seu pedido está sendo preparado para envio." },
            { OrderStatus.Shipped, "Seu pedido foi enviado! Acompanhe o código de rastreamento." },
            { OrderStatus.Delivered, "Seu pedido foi entregue com sucesso!" },
            { OrderStatus.Canceled, "Seu pedido foi cancelado." }
        };

        if (statusMessages.ContainsKey(orderEvent.NewStatus))
        {
            var subject = $"Pedido #{orderEvent.OrderId} - {statusMessages[orderEvent.NewStatus]}";
            var body = GenerateStatusChangeEmailBody(orderEvent);
            
            await _emailService.SendEmailAsync($"customer{orderEvent.CustomerId}@email.com", subject, body);
            Console.WriteLine($"📧 Email de mudança de status enviado - Status: {orderEvent.NewStatus}");
        }
    }

    public async Task OnPaymentProcessedAsync(PaymentProcessedEvent orderEvent)
    {
        if (orderEvent.PaymentSuccess)
        {
            var subject = $"Pedido #{orderEvent.OrderId} - Pagamento Aprovado";
            var body = GeneratePaymentSuccessEmailBody(orderEvent);
            
            await _emailService.SendEmailAsync($"customer{orderEvent.CustomerId}@email.com", subject, body);
            Console.WriteLine($"📧 Email de pagamento aprovado enviado - Valor: R$ {orderEvent.PaymentAmount:F2}");
        }
        else
        {
            var subject = $"Pedido #{orderEvent.OrderId} - Problema no Pagamento";
            var body = GeneratePaymentFailureEmailBody(orderEvent);
            
            await _emailService.SendEmailAsync($"customer{orderEvent.CustomerId}@email.com", subject, body);
            Console.WriteLine($"📧 Email de problema no pagamento enviado");
        }
    }

    public async Task OnOrderCancelledAsync(OrderCancelledEvent orderEvent)
    {
        var subject = $"Pedido #{orderEvent.OrderId} - Cancelamento Confirmado";
        var body = GenerateOrderCancelledEmailBody(orderEvent);
        
        await _emailService.SendEmailAsync($"customer{orderEvent.CustomerId}@email.com", subject, body);
        Console.WriteLine($"📧 Email de cancelamento enviado - Motivo: {orderEvent.CancellationReason}");
    }

    #endregion

    private string GenerateOrderCreatedEmailBody(OrderCreatedEvent orderEvent)
    {
        return $@"
            Olá {orderEvent.CustomerName},
            
            Seu pedido #{orderEvent.OrderId} foi criado com sucesso!
            
            Itens do pedido:
            {string.Join("\n", orderEvent.Items.Select(i => $"- {i.ProductName} (Qtd: {i.Quantity}) - R$ {i.TotalPrice:F2}"))}
            
            Valor total: R$ {orderEvent.OrderAmount:F2}
            Data do pedido: {orderEvent.EventDate:dd/MM/yyyy HH:mm}
            
            Obrigado por sua compra!
        ";
    }

    private string GenerateStatusChangeEmailBody(OrderStatusChangedEvent orderEvent)
    {
        return $@"
            Seu pedido #{orderEvent.OrderId} teve o status alterado.
            
            Status anterior: {orderEvent.PreviousStatus}
            Novo status: {orderEvent.NewStatus}
            Data da alteração: {orderEvent.EventDate:dd/MM/yyyy HH:mm}
            
            {(string.IsNullOrEmpty(orderEvent.ChangeReason) ? "" : $"Motivo: {orderEvent.ChangeReason}")}
            
            Continue acompanhando seu pedido!
        ";
    }

    private string GeneratePaymentSuccessEmailBody(PaymentProcessedEvent orderEvent)
    {
        return $@"
            Seu pagamento foi processado com sucesso!
            
            Pedido: #{orderEvent.OrderId}
            Valor pago: R$ {orderEvent.PaymentAmount:F2}
            Método de pagamento: {orderEvent.PaymentMethod}
            ID da transação: {orderEvent.TransactionId}
            Data do pagamento: {orderEvent.EventDate:dd/MM/yyyy HH:mm}
            
            Seu pedido agora será processado.
        ";
    }

    private string GeneratePaymentFailureEmailBody(PaymentProcessedEvent orderEvent)
    {
        return $@"
            Houve um problema com seu pagamento.
            
            Pedido: #{orderEvent.OrderId}
            Valor: R$ {orderEvent.PaymentAmount:F2}
            Método de pagamento: {orderEvent.PaymentMethod}
            
            Por favor, tente novamente ou entre em contato conosco.
        ";
    }

    private string GenerateOrderCancelledEmailBody(OrderCancelledEvent orderEvent)
    {
        return $@"
            Seu pedido #{orderEvent.OrderId} foi cancelado.
            
            Motivo: {orderEvent.CancellationReason}
            Data do cancelamento: {orderEvent.EventDate:dd/MM/yyyy HH:mm}
            
            {(orderEvent.RefundRequired ? $"Reembolso de R$ {orderEvent.RefundAmount:F2} será processado em até 5 dias úteis." : "")}
            
            Esperamos vê-lo novamente em breve!
        ";
    }
}

// Interface de serviço de email
public interface IEmailService
{
    Task SendEmailAsync(string toEmail, string subject, string body);
}

// Implementação simulada do serviço de email
public class EmailService : IEmailService
{
    public async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        // Simulação de envio de email
        await Task.Delay(100); // Simula latência de envio
        Console.WriteLine($"[EMAIL SIMULADO] Para: {toEmail} | Assunto: {subject}");
    }
}
