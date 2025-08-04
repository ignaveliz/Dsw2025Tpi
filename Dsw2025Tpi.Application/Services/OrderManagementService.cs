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
        if (string.IsNullOrEmpty(request.CustomerId) || string.IsNullOrWhiteSpace(request.CustomerId))
            throw new ArgumentException("El ID del cliente es obligatorio.");

        var customerId = Guid.Parse(request.CustomerId);

        var customer = await _repository.GetById<Customer>(customerId);
        if (customer is null)
            throw new EntityNotFoundException("El cliente no existe.");

        if (string.IsNullOrWhiteSpace(request.ShippingAddress) || string.IsNullOrWhiteSpace(request.BillingAddress))
            throw new ArgumentException("Direcciones de envío y facturación son obligatorias.");

        var totalAmount = 0m;
        var orderItems = new List<OrderItem>();
        var stockUpdates = new List<(Product product, int Quantity)>();

        foreach (var item in request.OrderItems)
        {    
            if (string.IsNullOrEmpty(item.ProductId))
                throw new ArgumentException("El ID del producto es obligatorio.");

            var productId = Guid.Parse(item.ProductId);

            var product = await _repository.GetById<Product>(productId);
            if (product is null)
                throw new EntityNotFoundException($" No existe el Producto con id: {item.ProductId}.");

            if (!product.IsActive)
                throw new EntityNotActive($"No está activo el producto con id: {item.ProductId}.");

            if (item.Quantity <= 0) throw new ArgumentException("La cantidad del producto debe ser mayor a cero.");

            if (product.StockQuantity < item.Quantity)
                throw new InsufficientStockException($"Stock insuficiente para {product.Name}.");

            stockUpdates.Add((product, product.StockQuantity - item.Quantity));

            var orderItem = new OrderItem 
            {
                ProductId = productId,
                Quantity = item.Quantity,
                UnitPrice = product.CurrentUnitPrice
            };

            orderItems.Add(orderItem);

            totalAmount += orderItem.SubTotal;
        }

        if (orderItems.Count == 0)
            throw new NoContentException("No se han agregado productos al pedido.");

        var order = new Order
        {
            CustomerId = customerId,
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
            order.Status.ToString(),
            order.TotalAmount,
            responseItems
        );
    }

    public async Task<IEnumerable<OrderModel.OrderResponse>> GetOrders(string? status, Guid? customerId, int pageNumber, int pageSize)
    {
        if (pageNumber < 1)
            throw new ArgumentException("El numero de pagina debe ser mayor o igual a 1.");

        if (pageSize < 1 || pageSize > 100)
            throw new ArgumentException("El tamaño de pagina debe estar entre 1 y 100.");

        var orders = await _repository.GetAll<Order>();
        var items = await _repository.GetAll<OrderItem>();

        if (orders is null || !orders.Any())
            throw new NoContentException("No hay órdenes cargadas en el sistema.");

        if (!string.IsNullOrEmpty(status))
        {
            if (!Enum.TryParse<OrderStatus>(status, true, out var parsedStatus))
            {
                throw new ArgumentException($"El estado '{status}' no es un estado de orden válido.");
            }

            orders = orders.Where(o => o.Status == parsedStatus).ToList();
        }

        if (customerId.HasValue)
        {
            orders = orders.Where(o => o.CustomerId == customerId.Value).ToList();
        }

        var paginatedOrders = orders
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var orderResponses = paginatedOrders.Select(order =>
        {
            var orderItems = items!
                .Where(i => i.OrderId == order.Id)
                .Select(i => new OrderModel.OrderItemResponse(
                    i.ProductId,
                    i.Quantity,
                    i.UnitPrice,
                    i.SubTotal))
                .ToList();

            var totalAmount = orderItems.Sum(i => i.SubTotal);

            return new OrderModel.OrderResponse(
                order.Id,
                order.Date,
                order.CustomerId,
                order.ShippingAddress!,
                order.BillingAddress!,
                order.Notes!,
                order.Status.ToString(),
                totalAmount,
                orderItems
            );
        }).ToList();

        return orderResponses;
    }
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
                i.SubTotal))
            .ToList();

        var totalAmount = orderItems.Sum(i => i.SubTotal);

        return new OrderModel.OrderResponse(
            order.Id,
            order.Date,
            order.CustomerId,
            order.ShippingAddress!,
            order.BillingAddress!,
            order.Notes!,
            order.Status.ToString(),
            totalAmount,
            orderItems
        );
    }

    public async Task<OrderModel.OrderResponse> UpdateOrderStatus(Guid id, string newStatus)
    {
        var order = await _repository.GetById<Order>(id);
        if (order is null)
            throw new EntityNotFoundException($"No existe una orden con el ID {id}.");

        var statusOld = order.Status;

        var status = newStatus.ToUpper();

        switch (status)
        {
            case "PROCESSING": 
                order.Status = OrderStatus.Processing;
                break;

            case "PENDING":
                order.Status = OrderStatus.Pending;
                break;

            case "SHIPPED":
                order.Status = OrderStatus.Shipped;
                break;

            case "DELIVERED":
                order.Status = OrderStatus.Delivered;
                break;

            case "CANCELLED":
                order.Status = OrderStatus.Cancelled;
                break;

            default:
                throw new ArgumentException("Estado Invalido");

        }

        if (statusOld == order.Status)
            throw new ArgumentException($"la orden ya se encuentra en {statusOld}");

        await _repository.Update(order);

        return await GetOrderById(order.Id);
    }

}
