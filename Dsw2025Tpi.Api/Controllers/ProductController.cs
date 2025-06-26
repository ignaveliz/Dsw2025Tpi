using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Dsw2025Tpi.Api.Controllers;

[ApiController]
[Route("api/products")]
public class ProductController : ControllerBase
{
    private readonly ProductsManagementService _service;
    public ProductController(ProductsManagementService service)
    {
         _service = service;
    }

    [HttpPost()]
    public async Task<IActionResult> AddProduct([FromBody]ProductModel.ProductRequest request)
    {
        try
        {
            var product = await _service.AddProduct(request);
            return Created($"/api/products/{product.Id}",product);
        }
        catch (ArgumentException ae)
        {
            return BadRequest(ae.Message);
        }
        catch (ApplicationException de)
        {
            return Conflict(de.Message);
        }
        catch (Exception)
        {
            return Problem("Error al guardar el producto");
        }
    }

    [HttpGet()]
    public async Task<IActionResult> GetProducts()
    {
        var products = await _service.GetProducts();
        if (products == null || !products.Any()) return NoContent();
        return Ok(products);
    }
}
