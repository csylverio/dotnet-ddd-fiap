using Microsoft.AspNetCore.Mvc;
using FiapEcommerce.Api.DTOs;
using FiapEcommerce.Domain.PurchaseTransaction;
using FiapEcommerce.Domain.PurchaseTransaction.Financial;
using FiapEcommerce.Domain.PurchaseTransaction.Strategy;

namespace FiapEcommerce.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class OrderController : ControllerBase
{
    private readonly OrderServiceBasic _basicService;
    private readonly OrderServiceWithBehavioralPatterns _patternService;
    private readonly IOrderBuilder _orderBuilder;

    public OrderController(
        OrderServiceBasic basicService,
        OrderServiceWithBehavioralPatterns patternService,
        IOrderBuilder orderBuilder)
    {
        _basicService = basicService;
        _patternService = patternService;
        _orderBuilder = orderBuilder;
    }

    [HttpPost("basic/create")]
    public IActionResult CreateBasic([FromBody] OrderDTO orderDto)
    {
        var order = BuildOrder(orderDto);
        order = _basicService.Create(order);

        return Ok(new
        {
            Mode = "Básico",
            Id = order.Id,
            TotalAmount = order.TotalAmount
        });
    }

    [HttpPost("patterns/create")]
    public async Task<IActionResult> CreateWithPatterns([FromBody] OrderDTO orderDto)
    {
        var order = BuildOrder(orderDto);
        order = await _patternService.CreateAsync(order);

        return Ok(new
        {
            Mode = "Padrões",
            Id = order.Id,
            TotalAmount = order.TotalAmount
        });
    }

    [HttpPost("basic/finalize")]
    public IActionResult FinalizeBasic([FromBody] FinalizeOrderDTO finalizeDto)
    {
        var order = _basicService.GetById(finalizeDto.OrderId);
        if (order == null) return NotFound("Pedido não encontrado para o fluxo básico.");

        order = _basicService.FinalizeOrder(order, finalizeDto.CouponCode);
        return Ok(new
        {
            Mode = "Básico",
            order.Id,
            order.DiscountDetail?.BaseDiscount,
            order.DiscountDetail?.PromotionalDiscount,
            order.DiscountDetail?.FinalDiscount
        });
    }

    [HttpPost("patterns/finalize")]
    public async Task<IActionResult> FinalizeWithPatterns([FromBody] FinalizeOrderDTO finalizeDto)
    {
        var order = _patternService.GetById(finalizeDto.OrderId);
        if (order == null) return NotFound("Pedido não encontrado para o fluxo com padrões.");

        order = await _patternService.FinalizeOrderAsync(order, finalizeDto.CouponCode);
        return Ok(new
        {
            Mode = "Padrões",
            order.Id,
            order.DiscountDetail?.BaseDiscount,
            order.DiscountDetail?.PromotionalDiscount,
            order.DiscountDetail?.FinalDiscount
        });
    }

    [HttpPost("basic/make-payment")]
    public IActionResult MakePaymentBasic([FromBody] PaymentDTO paymentDto)
    {
        var order = _basicService.GetById(paymentDto.OrderId);
        if (order == null) return NotFound("Pedido não encontrado para o fluxo básico.");

        var result = _basicService.MakePayment(order, paymentDto.PaymentMethodId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("patterns/make-payment")]
    public async Task<IActionResult> MakePaymentWithPatterns([FromBody] PaymentDTO paymentDto)
    {
        var order = _patternService.GetById(paymentDto.OrderId);
        if (order == null) return NotFound("Pedido não encontrado para o fluxo com padrões.");

        var payment = new Payment
        {
            OrderId = order.Id,
            Amount = order.TotalAmount,
            PaymentMethodId = paymentDto.PaymentMethodId,
            CardNumber = paymentDto.CardNumber,
            LastFourDigits = paymentDto.LastFourDigits,
            InstallmentCount = paymentDto.Installments
        };

        var result = await _patternService.MakePaymentAsync(order, payment, paymentDto.PaymentType);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    private Order BuildOrder(OrderDTO orderDto)
    {
        var builder = _orderBuilder
            .SetCustomerId(orderDto.CustomerId)
            .SetPaymentMethod(orderDto.PaymentMethodId)
            .SetShippingMethod(orderDto.ShippingMethodId)
            .SetDiscount(orderDto.Discount);

        foreach (var item in orderDto.Items)
        {
            builder.AddItem(item.ProductId, item.Quantity);
        }

        return builder.Build();
    }
}
