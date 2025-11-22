using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Exceptions;
using Dsw2025Tpi.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dsw2025Tpi.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/products")]
public class ProductController : ControllerBase
{
    private readonly ProductsManagementService _service;
    private readonly ILogger<ProductController> _logger;
    public ProductController(ProductsManagementService service, ILogger<ProductController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpPost()]
    [Authorize(Roles = "Admin,Tester")]
    public async Task<IActionResult> AddProduct([FromBody] ProductModel.ProductRequest request)
    {
        _logger.LogInformation("Solicitud recibida POST /api/products");
        var product = await _service.AddProduct(request);
        return Created($"/api/products/{product.Id}", product);
    }

    [HttpGet()]
    [AllowAnonymous]
    public async Task<IActionResult> GetProducts()
    {
        _logger.LogInformation("Solicitud recibida GET /api/products");
        var products = await _service.GetProducts();
        return Ok(products);
    }

    [HttpGet("admin")]
    [Authorize(Roles = "Admin,Tester")]
    public async Task<IActionResult> GetAuthProducts([FromQuery] ProductModel.FilterProduct request)
    {
        _logger.LogInformation("Solicitud recibida GET /api/products/admin");
        var products = await _service.GetProducts(request);
        if (products == null)
        {
            Response.Headers.Append("X-Message", "There are no active products");
            return NoContent();
        }
        return Ok(products);
    }


    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetProductById(Guid id)
    {
        _logger.LogInformation($"Solicitud recibida GET /api/products/{id}");
        var product = await _service.GetProductById(id);
        return Ok(product);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Tester")]
    public async Task<IActionResult> UpdateProduct(Guid id, [FromBody] ProductModel.ProductRequest request)
    {
        _logger.LogInformation($"Solicitud recibida PUT /api/products/{id}");
        var product = await _service.UpdateProduct(id, request);
        return Ok(product);
    }

    [HttpPatch("{id}")]
    [Authorize(Roles = "Admin,Tester")]
    public async Task<IActionResult> DisableProduct(Guid id)
    {
        _logger.LogInformation($"Solicitud recibida PATCH /api/products/{id}");
        var result = await _service.DisableProduct(id);
        return NoContent();
    }
}
