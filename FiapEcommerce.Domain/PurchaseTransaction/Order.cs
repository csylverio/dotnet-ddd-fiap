using System;
using System.Linq;
using FiapEcommerce.Domain.CustomerRelationshipManagement;
using FiapEcommerce.Domain.PurchaseTransaction.Composite;
using FiapEcommerce.Domain.PurchaseTransaction.Financial;
using FiapEcommerce.Domain.PurchaseTransaction.State;

namespace FiapEcommerce.Domain.PurchaseTransaction;

public class Order
{
    public Order()
    {
    }

    public Order(Customer customer, decimal discount, List<Item> items)
    {
        Customer = customer;
        Discount = discount;
        Items = items;
    }

    public int Id { get; set; }

    public int CustomerId { get; set; }
    public Customer Customer { get; set; }
    public DateTime OrderDate { get; set; } = DateTime.Now;
    public OrderStatus Status { get; set; } = OrderStatus.Draft;

    // Relacionamento com itens (1 pedido → N itens)
    public List<Item> Items { get; set; } = new List<Item>();

    public decimal Discount { get; set; } = 0;

    public decimal TotalAmount => Math.Max(Items.Sum(item => item.TotalPrice) - Discount, 0);

    // Relacionamento com pagamentos (1 Order → N Payments para casos de retentativa)
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public DiscountDetail DiscountDetail { get; internal set; }

    public AccountingStatus AccountingStatus { get; set; }
    public string AccountingDocument { get; set; }
    public DateTime? AccountingDate { get; set; }
    public string AccountingMessage { get; set; }

    // State Pattern - Métodos para gerenciamento de estado
    public OrderTransitionResult ChangeStatus(OrderStatus newStatus, string reason = "")
    {
        return OrderStateMachine.ValidateAndTransition(this, newStatus, reason);
    }

    public bool CanTransitionTo(OrderStatus newStatus)
    {
        return OrderStateMachine.CanTransition(this.Status, newStatus);
    }

    public List<OrderStatus> GetAllowedNextStates()
    {
        return OrderStateMachine.GetAllowedNextStates(this.Status);
    }

    public List<string> GetAllowedActions()
    {
        return OrderStateMachine.GetAllowedActions(this.Status);
    }

    internal void UpdateStatus(OrderStatus newStatus)
    {
        this.Status = newStatus;
    }
}
