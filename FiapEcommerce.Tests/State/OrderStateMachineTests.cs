using System.Collections.Generic;
using FiapEcommerce.Domain.InventoryManagement;
using FiapEcommerce.Domain.PurchaseTransaction;
using Xunit;

namespace FiapEcommerce.Tests.State;

public class OrderStateMachineTests
{
    [Fact]
    public void Deve_Aplicar_Transicoes_De_Status_Via_State()
    {
        var order = new Order
        {
            CustomerId = 1,
            Items = new List<Item> { new(new Product { Id = 1, SalePrice = 100m, Name = "Mouse" }, 1) }
        };

        var finalizeResult = order.ChangeStatus(OrderStatus.AwaitingPayment, "finalizado");
        Assert.True(finalizeResult.Success);
        Assert.Equal(OrderStatus.Draft, finalizeResult.PreviousStatus);
        Assert.Equal(OrderStatus.AwaitingPayment, order.Status);

        var invalidResult = order.ChangeStatus(OrderStatus.Delivered, "pular etapas");
        Assert.False(invalidResult.Success);
        Assert.Equal(OrderStatus.AwaitingPayment, order.Status);
    }
}
