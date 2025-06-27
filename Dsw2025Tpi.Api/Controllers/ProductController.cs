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
        catch (InvalidFieldException ie)
        {
            return BadRequest(ie.Message);
        }
        catch (DuplicatedEntityException de)
        {
            return BadRequest(de.Message);
        }
    }

    [HttpGet()]
    public async Task<IActionResult> GetProducts()
    {
        try
        {
            var products = await _service.GetProducts();
            return Ok(products);
        }
        catch (NoContentException)
        {
            return NoContent();
        }

    }


    [HttpGet("{id}")]
    public async Task<IActionResult> GetProductById(Guid id)
    {
        try
        {
            var product = await _service.GetProductById(id);
            return Ok(product);
        }
        catch (EntityNotFoundException enfe)
        {
            return NotFound(enfe.Message);
        }

    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(Guid id, [FromBody] ProductModel.ProductRequest request)
    {
        try
        {
            var product = await _service.UpdateProduct(id, request);
            return Ok(product);
        }
        catch (EntityNotFoundException enfe)
        {
            return NotFound(enfe.Message);
        }
        catch (EntityNotActive nae)
        {
            return NotFound(nae.Message);
        }
        catch (InvalidFieldException ife)
        {
            return BadRequest(ife.Message);
        }
        catch(DuplicatedEntityException de)
        {
            return BadRequest(de.Message);
        }
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> DisableProduct(Guid id)
    {
        try
        {
            var result = await _service.DisableProduct(id);
            return NoContent();
        }
        catch (EntityNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (EntityNotActive nae)
        {
            return NotFound(nae.Message);
        }
    }

}
