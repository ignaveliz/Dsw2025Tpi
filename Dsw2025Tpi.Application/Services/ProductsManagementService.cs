using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Domain.Entities;
using Dsw2025Tpi.Domain.Interfaces;
using Dsw2025Tpi.Application.Exceptions;
using Microsoft.Extensions.Logging;

namespace Dsw2025Tpi.Application.Services;

public class ProductsManagementService
{
    private readonly IRepository _repository;
    private readonly ILogger<ProductsManagementService> _logger;

    public ProductsManagementService(IRepository repository,ILogger<ProductsManagementService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<ProductModel.ProductResponse?> GetProductById(Guid id)
    {
        _logger.LogInformation("Obteniendo producto con id: {ProductId}", id);
        var product = await _repository.GetById<Product>(id);

        if (product is null)
        {   _logger.LogWarning("No se encontro el producto con id: {ProductId}", id);
            throw new EntityNotFoundException($"No existe el producto con id: {id}"); 
        }

        _logger.LogInformation("Producto con id: {ProductId} obtenido exitosamente", id);
        return new ProductModel.ProductResponse(product.Id, product.Sku!, product.InternalCode!, product.Name!, product.Description!, product.CurrentUnitPrice, product.StockQuantity, product.IsActive);
    }

    public async Task<IEnumerable<ProductModel.ProductResponse>?> GetProducts()
    {
        _logger.LogInformation("Obteniendo lista de productos activos");
        var products = (await _repository.GetFiltered<Product>(p => p.IsActive))?
            .Select(p => new ProductModel.ProductResponse(
                p.Id,
                p.Sku!,
                p.InternalCode!,
                p.Name!,
                p.Description!,
                p.CurrentUnitPrice,
                p.StockQuantity,
                p.IsActive
            )).ToList();

        if (products is null || !products.Any())
        { 
            _logger.LogWarning("No hay productos cargados en el sistema.");
            throw new NoContentException("No hay productos cargados en el sistema."); 
        }
            
        _logger.LogInformation("Lista de productos activos obtenida exitosamente");
        return products;

    }

    public async Task<ProductModel.PaginationResponse?> GetProducts(ProductModel.FilterProduct request)
    {
        var isActive = request.Status == "true"
            ? (bool?)true : request.Status == "false"
            ? (bool?)false : null;
        _logger.LogInformation("Obteniendo lista de productos con filtros - Estado: {Status}, Busqueda: {Search}, Pagina: {PageNumber}, TamañoPagina: {PageSize}",
            request.Status, request.Search, request.PageNumber, request.PageSize);

        var activeProducts = await _repository.GetFiltered<Product>(p => (
            (isActive == null || p.IsActive == isActive) && string.IsNullOrEmpty(request.Search) || p.Name!.Contains(request.Search!)
        ));

        if (activeProducts is null || !activeProducts.Any()) throw new NoContentException("No products were found");

        var products = activeProducts.Select(p => new ProductModel.ProductResponse(
            p.Id,
            p.Sku!,
            p.InternalCode!,
            p.Name!,
            p.Description!,
            p.CurrentUnitPrice,
            p.StockQuantity,
            p.IsActive))
            .OrderBy(p => p.Sku)
            .Skip((request.PageNumber - 1) * request.PageSize ?? 0)
            .Take(request.PageSize ?? activeProducts.Count());

        return new ProductModel.PaginationResponse(products.ToList(), activeProducts.Count());
    }
    public async Task<ProductModel.ProductResponse> AddProduct(ProductModel.ProductRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Sku))
        {
            _logger.LogError("El Sku del producto es obligatorio");
            throw new ArgumentException("El Sku del producto es obligatorio");
        }
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            _logger.LogError("El nombre del producto es obligatorio");
            throw new ArgumentException("El nombre del producto es obligatorio");
        }
        if (request.CurrentUnitPrice <= 0)
        {
            _logger.LogError("El precio del producto debe ser mayor a cero");
            throw new ArgumentException("El precio del producto debe ser mayor a cero");
        }
        if (request.StockQuantity < 0)
        {
            _logger.LogError("La cantidad de stock del producto no puede ser negativa");
            throw new ArgumentException("La cantidad de stock del producto no puede ser negativa");
        }

        var exist = await _repository.First<Product>(p => p.Sku == request.Sku);
        if (exist != null)
        { 
            _logger.LogError("Ya existe un producto con el Sku {Sku}", request.Sku);
            throw new DuplicatedEntityException($"Ya existe un producto con el Sku {request.Sku}");
        }
        var product = new Product(request.Sku, request.InternalCode!, request.Name, request.Description!, request.CurrentUnitPrice, request.StockQuantity);
        await _repository.Add(product);
        _logger.LogInformation("Producto con Sku {Sku} agregado exitosamente", request.Sku);
        return new ProductModel.ProductResponse(product.Id, product.Sku!, product.InternalCode!, product.Name!, product.Description!, product.CurrentUnitPrice, product.StockQuantity, product.IsActive);
    }

    public async Task<ProductModel.ProductResponse> UpdateProduct(Guid id, ProductModel.ProductRequest request)
    {
        _logger.LogInformation("Actualizando producto con id: {ProductId}", id);
        var product = await _repository.GetById<Product>(id);
        if (product is null)
        {
            _logger.LogError("El producto con id: {ProductId} no existe", id);
            throw new EntityNotFoundException("El producto no existe");
        }
        if (!product.IsActive)
        {
            _logger.LogError("El producto con id: {ProductId} no esta habilitado", id);
            throw new EntityNotActiveException("El producto no esta habilitado");
        }
        if (string.IsNullOrWhiteSpace(request.Sku))
        {
            _logger.LogError("El Sku del producto es obligatorio");
            throw new ArgumentException("El Sku del producto es obligatorio");
        }
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            _logger.LogError("El nombre del producto es obligatorio");
            throw new ArgumentException("El nombre del producto es obligatorio");
        }
        if (request.CurrentUnitPrice <= 0)
        {
            _logger.LogError("El precio del producto debe ser mayor a cero");
            throw new ArgumentException("El precio del producto debe ser mayor a cero");
        }
        if (request.StockQuantity < 0)
        {
            _logger.LogError("La cantidad de stock del producto no puede ser negativa");
            throw new ArgumentException("La cantidad de stock del producto no puede ser negativa");
        }

        var exist = await _repository.First<Product>(p => p.Sku == request.Sku);
        if (exist != null && product.Sku != request.Sku)
        {
            _logger.LogError("Ya existe un producto con el Sku {Sku}", request.Sku);
            throw new DuplicatedEntityException($"Ya existe un producto con el Sku {request.Sku}");
        }
        product.Sku = request.Sku;
        product.InternalCode = request.InternalCode;
        product.Name = request.Name;
        product.Description = request.Description;
        product.CurrentUnitPrice = request.CurrentUnitPrice;
        product.StockQuantity = request.StockQuantity;

        await _repository.Update(product);
        _logger.LogInformation("Producto con id: {ProductId} actualizado exitosamente", id);
        return new ProductModel.ProductResponse(
            product.Id, product.Sku, product.InternalCode!, product.Name, product.Description!, product.CurrentUnitPrice, product.StockQuantity, product.IsActive);
    }

    public async Task<bool> DisableProduct(Guid id)
    {
        _logger.LogInformation("Deshabilitando producto con id: {ProductId}", id);
        var product = await _repository.GetById<Product>(id);
        if (product is null)
        {
            _logger.LogError("El producto con id: {ProductId} no existe", id);
            throw new EntityNotFoundException("El producto no existe");
        }
        if (!product.IsActive)
        {
            _logger.LogError("El producto con id: {ProductId} ya se encuentra deshabilitado", id);
            throw new EntityNotActiveException("El producto ya se encuentra actualmente deshabilitado");
        }
        product.IsActive = false;
        await _repository.Update(product);
        _logger.LogInformation("Producto con id: {ProductId} deshabilitado exitosamente", id);
        return true;
    }
}
