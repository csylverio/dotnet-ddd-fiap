git ad# Padrões Comportamentais - Design Patterns em .NET

Este projeto demonstra a implementação de 4 padrões comportamentais do GoF:

## 📋 Padrões Implementados

### 1. 🔄 STATE PATTERN
**Localização:** `FiapEcommerce.Domain/PurchaseTransaction/State/`

**Problema Resolvido:** Gerenciamento de transições de status de pedidos com validação

**Implementação:**
- `OrderStateMachine.cs` - Gerencia transições válidas entre estados
- `OrderTransitionResult.cs` - Resultado das transições
- Integrado na classe `Order.cs` com métodos como `ChangeStatus()` e `CanTransitionTo()`

**Exemplo de uso:**
```csharp
var result = order.ChangeStatus(OrderStatus.PaymentApproved, "Pagamento aprovado");
if (result.Success) {
    // Status alterado com sucesso
}
```

### 2. 🎯 STRATEGY PATTERN
**Localização:** `FiapEcommerce.Domain/PurchaseTransaction/Strategy/`

**Problema Resolvido:** Diferentes estratégias de processamento de pagamento

**Implementação:**
- `IPaymentProcessingStrategy.cs` - Interface comum
- `SinglePaymentStrategy.cs` - Pagamento à vista
- `InstallmentPaymentStrategy.cs` - Pagamento parcelado
- `PaymentStrategyContext.cs` - Context que escolhe a estratégia

**Exemplo de uso:**
```csharp
var result = await _paymentStrategyContext.ProcessPaymentAsync(order, payment, PaymentType.InstallmentPayment);
```

### 3. ⛓️ CHAIN OF RESPONSIBILITY
**Localização:** `FiapEcommerce.Domain/PurchaseTransaction/ChainOfResponsibility/`

**Problema Resolvido:** Validação em cadeia de pedidos

**Implementação:**
- `IOrderProcessingHandler.cs` - Interface do handler
- `BaseOrderProcessingHandler.cs` - Classe base abstrata
- `OrderValidationHandler.cs` - Validação básica
- `InventoryCheckHandler.cs` - Verificação de estoque
- `BusinessRulesHandler.cs` - Regras de negócio
- `OrderProcessingChain.cs` - Monta e executa a cadeia

**Exemplo de uso:**
```csharp
var result = await _orderProcessingChain.ProcessOrderAsync(order, "validate");
if (!result.Success) {
    throw new InvalidOperationException(result.Message);
}
```

### 4. 👀 OBSERVER PATTERN
**Localização:** `FiapEcommerce.Domain/PurchaseTransaction/DomainEvents/`

**Problema Resolvido:** Sistema de eventos desacoplado para notificações

**Implementação:**
- `IOrderEventPublisher.cs` - Interface do publisher
- `OrderEventPublisher.cs` - Implementação do publisher
- `OrderEvents.cs` - Eventos do domínio
- **Subscribers:**
  - `EmailNotificationSubscriber.cs` - Notificações por email
  - `InventoryUpdateSubscriber.cs` - Atualizações de estoque
  - `AuditLogSubscriber.cs` - Log de auditoria

**Exemplo de uso:**
```csharp
await _eventPublisher.PublishOrderCreatedAsync(orderCreatedEvent);
// Todos os subscribers registrados são notificados automaticamente
```

## 🔧 Configuração no Program.cs

```csharp
// Strategy Pattern
builder.Services.AddScoped<IPaymentProcessingStrategy, SinglePaymentStrategy>();
builder.Services.AddScoped<IPaymentProcessingStrategy, InstallmentPaymentStrategy>();
builder.Services.AddScoped<PaymentStrategyContext>();

// Chain of Responsibility
builder.Services.AddScoped<OrderProcessingChain>();

// Observer Pattern
builder.Services.AddSingleton<IOrderEventPublisher, OrderEventPublisher>();
builder.Services.AddScoped<EmailNotificationSubscriber>();
builder.Services.AddScoped<InventoryUpdateSubscriber>();
builder.Services.AddScoped<AuditLogSubscriber>();

// Registrar subscribers
eventPublisher.Subscribe(emailSubscriber);
eventPublisher.Subscribe(inventorySubscriber);
eventPublisher.Subscribe(auditSubscriber);
```

## 🚀 Serviço Principal Atualizado

**Arquivo:** `OrderServiceWithBehavioralPatterns.cs`

Integra todos os padrões comportamentais com os estruturais existentes:

```csharp
public async Task<Order> CreateAsync(Order order)
{
    // Chain of Responsibility - Validação
    var validationResult = await _orderProcessingChain.ProcessOrderAsync(order, "create");
    
    if (!validationResult.Success) {
        throw new InvalidOperationException($"Falha na validação: {validationResult.Message}");
    }

    _orderRepository.Add(order);

    // Observer Pattern - Publicar evento
    var orderCreatedEvent = CreateOrderCreatedEvent(order);
    await _eventPublisher.PublishOrderCreatedAsync(orderCreatedEvent);

    return order;
}

public async Task<PaymentResult> MakePaymentAsync(Order order, Payment payment, PaymentType paymentType)
{
    // State Pattern - Verificar se pode receber pagamento
    if (!order.GetAllowedActions().Contains("ProcessPayment")) {
        return new PaymentResult { Success = false, ErrorMessage = "Status inválido para pagamento" };
    }

    // Strategy Pattern - Processar pagamento
    var paymentResult = await _paymentStrategyContext.ProcessPaymentAsync(order, payment, paymentType);

    // State Pattern - Atualizar status
    if (paymentResult.Success && paymentResult.NextStep.HasValue) {
        var statusChangeResult = order.ChangeStatus(paymentResult.NextStep.Value, "Pagamento processado");
        
        if (statusChangeResult.Success) {
            // Observer Pattern - Publicar eventos
            await _eventPublisher.PublishPaymentProcessedAsync(paymentEvent);
            await _eventPublisher.PublishOrderStatusChangedAsync(statusEvent);
        }
    }

    return paymentResult;
}
```

## 📊 Benefícios dos Padrões Comportamentais

### State Pattern
- ✅ Previne transições inválidas de status
- ✅ Centraliza regras de estado
- ✅ Facilita adição de novos estados
- ✅ Melhora a auditoria de mudanças

### Strategy Pattern
- ✅ Flexibilidade para novos tipos de pagamento
- ✅ Validações específicas por estratégia
- ✅ Facilita testes unitários
- ✅ Permite configurações específicas (juros, parcelas)

### Chain of Responsibility
- ✅ Validações modulares e reutilizáveis
- ✅ Pipeline flexível de processamento
- ✅ Fácil adição de novas validações
- ✅ Melhor rastreabilidade de falhas

### Observer Pattern
- ✅ Notificações desacopladas
- ✅ Sistema de eventos escalável
- ✅ Fácil adição de novos subscribers
- ✅ Processamento assíncrono de eventos

## 🎯 Para a Próxima Aula

### Pontos de Destaque:
1. **Evolução Natural** - Como os padrões comportamentais complementam os estruturais
2. **Problemas Reais** - Cada padrão resolve um problema específico do domínio
3. **Integração Harmoniosa** - Todos os padrões trabalham juntos
4. **Flexibilidade** - Sistema muito mais extensível e manutenível

### Demonstrações Práticas:
1. Tentar mudança de status inválida (State Pattern)
2. Processar pagamento parcelado vs à vista (Strategy Pattern)  
3. Adicionar nova validação na cadeia (Chain of Responsibility)
4. Ver eventos sendo disparados em tempo real (Observer Pattern)

## 🏗️ Arquitetura Final

O projeto agora combina:
- **Estruturais:** Builder, Composite, Adapter, Factory, Facade
- **Comportamentais:** State, Strategy, Chain of Responsibility, Observer
- **Arquiteturais:** Clean Architecture, Dependency Injection, Repository Pattern

Resultado: Sistema robusto, flexível e altamente manutenível para ensino de Design Patterns!
