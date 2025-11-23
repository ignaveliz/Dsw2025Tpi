using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Exceptions;
using Dsw2025Tpi.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Dsw2025Tpi.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Dsw2025Tpi.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/orders")]
public class OrderController : ControllerBase
{
    private readonly OrderManagementService _service;
    private readonly ILogger<OrderController> _logger;
    public OrderController(OrderManagementService service, ILogger<OrderController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpPost()]
    [Authorize(Roles = "Usuario,Tester")]
    public async Task<IActionResult> CreateOrder([FromBody] OrderModel.OrderRequest request)
    {
        _logger.LogInformation("Creating a new order for CustomerId: {CustomerId}", request.CustomerId);
        var order = await _service.CreateOrder(request);
        return Created($"/api/orders/{order?.OrderId}", order);
    }

    [HttpGet()]
    [Authorize(Roles = "Usuario,Tester,Admin")]
    public async Task<IActionResult> GetOrders([FromQuery] OrderModel.FilterOrder request)
    {
        _logger.LogInformation("Retrieving orders with filters - Status: {Status}, CustomerId: {CustomerId}, PageNumber: {PageNumber}, PageSize: {PageSize}",
            request.Status, request.CustomerId, request.PageNumber, request.PageSize);
        var orders = await _service.GetOrders(request);
        return Ok(orders);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Usuario,Tester,Admin")]
    public async Task<IActionResult> GetOrderById(Guid id)
    {
        _logger.LogInformation("Retrieving order with ID: {OrderId}", id);
        var order = await _service.GetOrderById(id);
        return Ok(order);
    }

    [HttpPut("{id}/status")]
    [Authorize(Roles = "Admin,Tester")]
    public async Task<IActionResult> UpdateOrderStatus(Guid id, [FromBody] OrderModel.UpdateOrderStatusRequest request)
    {
        _logger.LogInformation("Updating status for order ID: {OrderId} to new status: {NewStatus}", id, request.NewStatus);
        var updatedOrder = await _service.UpdateOrderStatus(id, request.NewStatus!);
        return Ok(updatedOrder);
    }

}
