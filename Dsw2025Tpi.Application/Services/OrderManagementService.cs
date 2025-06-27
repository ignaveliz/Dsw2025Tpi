using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Exceptions;
using Dsw2025Tpi.Domain.Entities;
using Dsw2025Tpi.Domain.Interfaces;

namespace Dsw2025Tpi.Application.Services;

public class OrderManagementService
{
    private readonly IRepository _repository;

    public OrderManagementService(IRepository repository)
    {
        _repository = repository;
    }

    public async Task<OrderModel.OrderResponse?> CreateOrder(OrderModel.OrderRequest request)
    {
        var customer = await _repository.GetById<Customer>(request.CustomerId);
        if (customer is null)
            throw new EntityNotFoundException("El cliente no existe.");

        if (string.IsNullOrWhiteSpace(request.ShippingAddress) || string.IsNullOrWhiteSpace(request.BillingAddress))
            throw new InvalidFieldException("Direcciones de envío y facturación son obligatorias.");

        var totalAmount = 0m;
        var orderItems = new List<OrderItem>();
        var stockUpdates = new List<(Product product, int Quantity)>();

        foreach (var item in request.OrderItems)
        {
            var product = await _repository.GetById<Product>(item.ProductId);
            if (product is null || !product.IsActive)
                throw new InvalidEntityException($"Producto {item.ProductId} inválido.");

            if(string.IsNullOrWhiteSpace(item.Name))
                throw new InvalidFieldException("El nombre del producto es obligatorio.");

            if (item.Quantity <= 0) throw new InvalidFieldException("La cantidad del producto debe ser mayor a cero.");

            if (item.UnitPrice < 0)
                throw new InvalidFieldException("El precio unitario del producto no puede ser negativo.");

            if (product.StockQuantity < item.Quantity)
                throw new InsufficientStockException($"Stock insuficiente para {product.Name}.");

            stockUpdates.Add((product,product.StockQuantity - item.Quantity));

            var subTotal = item.UnitPrice * item.Quantity;
            totalAmount += subTotal;

            orderItems.Add(new OrderItem
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                SubTotal = subTotal 
            });
        }

        if (orderItems.Count == 0)
            throw new NoContentException("No se han agregado productos al pedido.");
        
        var order = new Order
        {
            CustomerId = request.CustomerId,
            ShippingAddress = request.ShippingAddress,
            BillingAddress = request.BillingAddress,
            Date = DateTime.UtcNow,
            Notes = request.Notes ?? String.Empty,
            Status = OrderStatus.Pending,
            TotalAmount = totalAmount
        };

        await _repository.Add(order);

        foreach (var item in orderItems)
        {
            item.OrderId = order.Id;
            await _repository.Add(item);
        }

        foreach (var (product, newStock) in stockUpdates)
        {
            product.StockQuantity = newStock;
            await _repository.Update(product);
        }

        var responseItems = orderItems.Select(i => new OrderModel.OrderItemResponse(
            i.ProductId,
            i.Quantity,
            i.UnitPrice,
            i.SubTotal
        )).ToList();

        return new OrderModel.OrderResponse(
            order.Id,
            order.Date,
            order.ShippingAddress,
            order.BillingAddress,
            order.Notes,
            order.Status,
            order.TotalAmount,
            responseItems
        );
    }
}
