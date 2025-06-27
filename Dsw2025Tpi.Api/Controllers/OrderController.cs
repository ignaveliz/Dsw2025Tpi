using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Services;
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
        catch (ArgumentException ae)
        {
            return BadRequest(ae.Message);
        }
        catch (ApplicationException de)
        {
            return Conflict(de.Message);
        }
        catch (Exception ex)
        {
            return Problem(detail: ex.InnerException?.Message ?? ex.Message, title: "Excepción interna", statusCode: 500);
        }
    }
}
