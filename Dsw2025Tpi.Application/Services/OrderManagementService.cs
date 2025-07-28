using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
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
            if (product is null)
                throw new InvalidEntityException($" No existe el Producto con id: {item.ProductId}.");

            if (!product.IsActive)
                throw new InvalidEntityException($"No está activo el producto con id: {item.ProductId}.");

            if (string.IsNullOrWhiteSpace(item.Name))
                throw new InvalidFieldException("El nombre del producto es obligatorio.");

            if (item.Quantity <= 0) throw new InvalidFieldException("La cantidad del producto debe ser mayor a cero.");

            if (item.UnitPrice < 0)
                throw new InvalidFieldException("El precio unitario del producto no puede ser negativo.");

            if (product.StockQuantity < item.Quantity)
                throw new InsufficientStockException($"Stock insuficiente para {product.Name}.");

            stockUpdates.Add((product, product.StockQuantity - item.Quantity));

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
            order.CustomerId,
            order.ShippingAddress,
            order.BillingAddress,
            order.Notes,
            order.Status,
            order.TotalAmount,
            responseItems
        );
    }

    public async Task<IEnumerable<OrderModel.OrderResponse>?> GetOrders()
    {
        var orders = await _repository.GetAll<Order>();
        var items = await _repository.GetAll<OrderItem>();

        if (orders is null || !orders.Any())
            throw new NoContentException("No hay oredenes cargadas en el sistema.");

        var orderResponses = orders.Select(order =>
        {
            var orderItems = items!
                .Where(i => i.OrderId == order.Id)
                .Select(i => new OrderModel.OrderItemResponse(
                    i.ProductId,
                    i.Quantity,
                    i.UnitPrice,
                    i.Quantity * i.UnitPrice))
                .ToList();

            var totalAmount = orderItems!.Sum(i => i.SubTotal);

            return new OrderModel.OrderResponse(
                order.Id,
                order.Date,
                order.CustomerId,
                order.ShippingAddress!,
                order.BillingAddress!,
                order.Notes!,
                order.Status,
                totalAmount,
                orderItems
            );
        }).ToList();

        return orderResponses;
    }
    ///8
    public async Task<OrderModel.OrderResponse> GetOrderById(Guid id)
    {
        var order = await _repository.GetById<Order>(id);
        if (order is null)
            throw new EntityNotFoundException($"No existe una orden con el ID {id}.");

        var items = await _repository.GetAll<OrderItem>();
        var orderItems = items!
            .Where(i => i.OrderId == order.Id)
            .Select(i => new OrderModel.OrderItemResponse(
                i.ProductId,
                i.Quantity,
                i.UnitPrice,
                i.Quantity * i.UnitPrice))
            .ToList();

        var totalAmount = orderItems.Sum(i => i.SubTotal);

        return new OrderModel.OrderResponse(
            order.Id,
            order.Date,
            order.CustomerId,
            order.ShippingAddress!,
            order.BillingAddress!,
            order.Notes!,
            order.Status,
            totalAmount,
            orderItems
        );
    }

    //////////////////////////////////////////////////////////////////////////////////
    //9
    public async Task<OrderModel.OrderResponse> UpdateOrderStatus(Guid id, OrderStatus newStatus)
    {
        var order = await _repository.GetById<Order>(id);
        if (order is null)
            throw new EntityNotFoundException($"No existe una orden con el ID {id}.");

     
        if (order.Status == newStatus)
            return await GetOrderById(order.Id);

        
        order.Status = newStatus;

        await _repository.Update(order);

        return await GetOrderById(order.Id);
    }
    



}
