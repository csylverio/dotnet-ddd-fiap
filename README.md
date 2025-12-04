# FiapEcommerce – DDD Tático + Padrões Comportamentais

Este repositório apresenta um cenário simples de pedidos dentro de um e-commerce fictício. A ideia é utilizar o domínio como base para demonstrar, em uma live hands-on, como organizar serviços táticos de DDD e aplicar padrões comportamentais.

## Cenário de negócio
- **Pedido** formado por cliente, itens, descontos, pagamento e integração contábil.
- **Serviço básico (`OrderServiceBasic`)**: toda a lógica acoplada em uma única classe para mostrar o "anti-exemplo".
- **Serviço com padrões (`OrderServiceWithBehavioralPatterns`)**: divide responsabilidades usando Composite, Strategy, Chain of Responsibility, State e Domain Events.

## Onde cada padrão aparece
| Padrão | Pasta / Namespace | Papel no domínio |
| --- | --- | --- |
| Composite | `FiapEcommerce.Domain/PurchaseTransaction/Composite` | Composição de regras de desconto (folhas + composites). |
| Strategy | `FiapEcommerce.Domain/PurchaseTransaction/Strategy` | Estratégias de processamento de pagamento (à vista e parcelado). |
| Chain of Responsibility | `FiapEcommerce.Domain/PurchaseTransaction/ChainOfResponsibility` | Pipeline de validação de pedidos antes de avançar o fluxo. |
| State | `FiapEcommerce.Domain/PurchaseTransaction/State` | Máquina de estados do pedido com transições permitidas. |
| Domain Events / Observer | `FiapEcommerce.Domain/PurchaseTransaction/DomainEvents` | Publicação e assinatura de eventos de pedido (e-mail, estoque, auditoria). |

As implementações concretas de repositórios e integrações se encontram em `FiapEcommerce.Infra`, enquanto DTOs e endpoints ficam em `FiapEcommerce.Api`.

## Endpoints para comparação dos serviços
O `OrderController` expõe endpoints específicos para cada abordagem:

| Fluxo | Endpoint | Descrição |
| --- | --- | --- |
| Básico | `POST /api/order/basic/create` | Cria pedido com o serviço acoplado. |
| Básico | `POST /api/order/basic/finalize` | Finaliza pedido aplicando lógica manual de desconto/contabilidade. |
| Básico | `POST /api/order/basic/make-payment` | Processa pagamento direto no serviço. |
| Padrões | `POST /api/order/patterns/create` | Cria pedido passando pela cadeia de validação e eventos. |
| Padrões | `POST /api/order/patterns/finalize` | Finaliza pedido usando Composite + State + eventos. |
| Padrões | `POST /api/order/patterns/make-payment` | Processa pagamento via Strategy (atribuir `paymentType` e `installments` no DTO). |

O DTO `PaymentDTO` permite informar `paymentType` (SinglePayment ou InstallmentPayment) e `installments` para exercitar as estratégias.

## Como executar
Use os comandos abaixo no diretório raiz do repositório:

```bash
# Restaurar dependências
dotnet restore

# Compilar solução (opcionalmente use --no-restore se já restaurou)
dotnet build FiapEcommerce.sln -c Debug

# Executar a API (Swagger habilitado em Development)
dotnet run --project FiapEcommerce.Api/FiapEcommerce.Api.csproj -c Debug --no-build

# Executar testes
dotnet test FiapEcommerce.Tests/FiapEcommerce.Tests.csproj -c Debug --no-build
```

Durante a demo é possível chamar tanto os endpoints básicos quanto os com padrões para comparar mensagens e logs (eventos, cadeias e mudanças de estado são escritos no console).

## Testes didáticos
Foi adicionado o projeto `FiapEcommerce.Tests` com cenários focados em demonstração:
- Composite de descontos (combinação de regras básicas e promocionais).
- Strategy de pagamento (à vista x parcelado).
- Chain of Responsibility (ordem dos handlers).
- State (transições válidas e inválidas).
- Domain Events / Observer (subscribers notificados).

Execute com:
```bash
dotnet test FiapEcommerce.Tests/FiapEcommerce.Tests.csproj
```

A solução (`FiapEcommerce.sln`) já referencia o projeto de testes para facilitar o workflow em IDE.
