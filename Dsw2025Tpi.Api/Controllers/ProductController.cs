using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Exceptions;
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
    public async Task<IActionResult> AddProduct([FromBody] ProductModel.ProductRequest request)
    {
        try
        {
            var product = await _service.AddProduct(request);
            return Created($"/api/products/{product.Id}", product);
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


    [HttpGet("{id}")]
    public async Task<IActionResult> GetProductById(Guid id)
    {
        var product = await _service.GetProductById(id);
        if (product is null)
            return NotFound($"Producto con ID {id} no encontrado.");

        return Ok(product);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(Guid id, [FromBody] ProductModel.ProductRequest request)
    {
        try
        {
            var product = await _service.UpdateProduct(id, request);
            if (product is null)
                return NotFound($"No se encontró un producto con ID {id}.");

            return Ok(product);
        }
        catch (ArgumentException ae)
        {
            return BadRequest(ae.Message);
        }
        catch (Exception)
        {
            return Problem("Error al actualizar el producto.");
        }
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> DisableProduct(Guid id)
    {
        try
        {
            var result = await _service.DisableProduct(id);
            return Ok(new { message = "Producto deshabilitado correctamente." });
        }
        catch (EntityNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception)
        {
            return Problem("Error al deshabilitar el producto.");
        }
    }

}
