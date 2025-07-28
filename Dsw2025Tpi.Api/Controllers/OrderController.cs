using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Exceptions;
using Dsw2025Tpi.Application.Services;
using Dsw2025Tpi.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Dsw2025Tpi.Api.Controllers;

[ApiController]
[Route("api/orders")]
public class OrderController : ControllerBase
{
    private readonly OrderManagementService _service;
    public OrderController(OrderManagementService service)
    {
        _service = service;
    }

    [HttpPost()]
    public async Task<IActionResult> CreateOrder([FromBody] OrderModel.OrderRequest request)
    {
        try
        {
            var order = await _service.CreateOrder(request);
            return Created($"/api/orders/{order.OrderId}", order);
        }
        catch (EntityNotFoundException nfe)
        {
            return BadRequest(nfe.Message);
        }
        catch (InvalidEntityException ie)
        {
            return BadRequest(ie.Message);
        }
        catch (InsufficientStockException ise)
        {
            return BadRequest(ise.Message);
        }
        catch(NoContentException nce)
        {
            return BadRequest(nce.Message);
        }
        catch (InvalidFieldException ife)
        {
            return BadRequest(ife.Message);
        }
    }

    [HttpGet()]


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
    }
    ///punto 8
    [HttpGet("{id}")]
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

    /// punto 9
    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateOrderStatus(Guid id, [FromBody] UpdateOrderStatusRequest request)
    {
        try
        {
            var order = await _service.GetOrderById(id);
            if (order == null)
            {
                return NotFound($"No existe la orden con id: {id}");
            }

            if (!Enum.IsDefined(typeof(OrderStatus), request.NewStatus))
            {
                return BadRequest("El estado proporcionado no es válido.");
            }


            var updatedOrder = await _service.UpdateOrderStatus(id, request.NewStatus);
            return Ok(updatedOrder);
        }
        catch (InvalidEntityException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error interno del servidor: {ex.Message}");
        }
    }

}
