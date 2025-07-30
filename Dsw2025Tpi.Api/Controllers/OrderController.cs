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
    public OrderController(OrderManagementService service)
    {
        _service = service;
    }

    [HttpPost()]
    [Authorize(Roles = "Usuario,Tester")]
    public async Task<IActionResult> CreateOrder([FromBody] OrderModel.OrderRequest request)
    {
        try
        {
            var order = await _service.CreateOrder(request);
            return Created($"/api/orders/{order?.OrderId}", order);
        }
        catch (EntityNotFoundException nfe)
        {
            return BadRequest(nfe.Message);
        }
        catch (ArgumentException ife)
        {
            return BadRequest(ife.Message);
        }
        catch (EntityNotActive nae)
        {
            return BadRequest(nae.Message);
        }
        catch (InsufficientStockException ise)
        {
            return BadRequest(ise.Message);
        }
        catch(NoContentException nce)
        {
            return BadRequest(nce.Message);
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpGet()]
    [Authorize(Roles = "Usuario,Tester,Admin")]
    public async Task<IActionResult> GetOrders()
    {
        try
        {
            var orders = await _service.GetOrders();
            return Ok(orders);
        }
        catch (NoContentException)
        {
            return StatusCode(500); 
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }
    
    [HttpGet("{id}")]
    [Authorize(Roles = "Usuario,Tester,Admin")]
    public async Task<IActionResult> GetOrderById(Guid id)
    {
        try
        {
            var order = await _service.GetOrderById(id);
            return Ok(order); 
        }
        catch (EntityNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPut("{id}/status")]
    [Authorize(Roles = "Admin,Tester")]
    public async Task<IActionResult> UpdateOrderStatus(Guid id, [FromBody] UpdateOrderStatusRequest request)
    {
        try
        {
            var updatedOrder = await _service.UpdateOrderStatus(id, request.NewStatus);
            return Ok(updatedOrder);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (EntityNotFoundException)
        {
            return NotFound($"No existe una orden con el ID {id}.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error interno del servidor: {ex.Message}");
        }
    }

}
