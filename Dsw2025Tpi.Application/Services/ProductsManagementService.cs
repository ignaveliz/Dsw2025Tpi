using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Domain.Entities;
using Dsw2025Tpi.Domain.Interfaces;
using Dsw2025Tpi.Application.Exceptions;

namespace Dsw2025Tpi.Application.Services;

public class ProductsManagementService
{
    private readonly IRepository _repository;

    public ProductsManagementService(IRepository repository)
    {
        _repository = repository;
    }

    public async Task<ProductModel.ProductResponse?> GetProductById(Guid id)
    {
        var product = await _repository.GetById<Product>(id);
        return product != null ?
            new ProductModel.ProductResponse(product.Id,product.Sku,product.InternalCode,product.Name,product.Description,product.CurrentUnitPrice,product.StockQuantity,product.IsActive) :
            null;
    }

    public async Task<IEnumerable<ProductModel.ProductResponse>?> GetProducts()
    {
        return (await _repository
            .GetFiltered<Product>(p => p.IsActive))?
            .Select(p => new ProductModel.ProductResponse(p.Id, p.Sku, p.InternalCode, p.Name, p.Description, p.CurrentUnitPrice, p.StockQuantity, p.IsActive));
    }

    public async Task<ProductModel.ProductResponse> AddProduct(ProductModel.ProductRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Sku) ||
            string.IsNullOrWhiteSpace(request.Name) ||
            request.CurrentUnitPrice < 0 || request.StockQuantity < 0
            )
        {
            throw new ArgumentException("Valores para el producto no válidos");
        }

        var exist = await _repository.First<Product>(p => p.Sku == request.Sku);
        if (exist != null) throw new DuplicatedEntityException($"Ya existe un producto con el Sku {request.Sku}");
        var product = new Product(request.Sku,request.InternalCode,request.Name,request.Description,request.CurrentUnitPrice,request.StockQuantity);
        await _repository.Add(product);
        return new ProductModel.ProductResponse(product.Id, product.Sku, product.InternalCode, product.Name, product.Description, product.CurrentUnitPrice, product.StockQuantity, product.IsActive);
    }

    public async Task<ProductModel.ProductResponse> UpdateProduct(Guid id, ProductModel.ProductRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Sku) ||
            string.IsNullOrWhiteSpace(request.Name) ||
            request.CurrentUnitPrice < 0 || request.StockQuantity < 0
            )
        {
            throw new ArgumentException("Valores para el producto no válidos");
        }

        var product = await _repository.GetById<Product>(id);
        if (product is null || !product.IsActive) throw new EntityNotFoundException("Producto no encontrado");

        product.Sku = request.Sku;
        product.InternalCode = request.InternalCode;
        product.Name = request.Name;
        product.Description = request.Description;
        product.CurrentUnitPrice = request.CurrentUnitPrice;
        product.StockQuantity = request.StockQuantity;

        await _repository.Update(product);

        return new ProductModel.ProductResponse(
            product.Id, product.Sku, product.InternalCode, product.Name, product.Description, product.CurrentUnitPrice, product.StockQuantity, product.IsActive);
    }

    public async Task<bool> DisableProduct(Guid id)
    {
        var product = await _repository.GetById<Product>(id);
        if (product is null || !product.IsActive) throw new EntityNotFoundException("Producto no encontrado");

        product.IsActive = false;
        await _repository.Update(product);

        return true;
    }
}
