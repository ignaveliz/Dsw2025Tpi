using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Exceptions;
using Dsw2025Tpi.Application.Services;
using Microsoft.AspNetCore.Authorization;
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
}
