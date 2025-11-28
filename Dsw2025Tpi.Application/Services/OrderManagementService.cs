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
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace Dsw2025Tpi.Application.Services;

public class OrderManagementService
{
    private readonly IRepository _repository;
    private readonly ILogger<OrderManagementService> _logger;

    public OrderManagementService(IRepository repository,ILogger<OrderManagementService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<OrderModel.OrderResponse?> CreateOrder(OrderModel.OrderRequest request)
    {
        _logger.LogInformation("Iniciando creación de orden para el cliente {CustomerId}", request.CustomerId);

        if (string.IsNullOrEmpty(request.CustomerId) || string.IsNullOrWhiteSpace(request.CustomerId))
        {
            _logger.LogError("El ID del cliente es nulo o vacío.");
            throw new ArgumentException("El ID del cliente es obligatorio.");
        }

        var customerId = Guid.Parse(request.CustomerId);

        var customer = await _repository.GetById<Customer>(customerId);
        if (customer is null)
        {
            _logger.LogError("Cliente con ID {CustomerId} no encontrado.", request.CustomerId);
            throw new ArgumentException("El cliente no existe.");
        }

        if (string.IsNullOrWhiteSpace(request.ShippingAddress) || string.IsNullOrWhiteSpace(request.BillingAddress))
        {
            _logger.LogError("Direcciones de envío o facturación son nulas o vacías.");
            throw new ArgumentException("Direcciones de envío y facturación son obligatorias.");
        }

        var totalAmount = 0m;
        var orderItems = new List<OrderItem>();
        var stockUpdates = new List<(Product product, int Quantity)>();

        foreach (var item in request.OrderItems)
        {    
            if (string.IsNullOrEmpty(item.ProductId))
            {
                _logger.LogError("El ID del producto es nulo o vacío.");
                throw new ArgumentException("El ID del producto es obligatorio.");
            }

            var productId = Guid.Parse(item.ProductId);

            var product = await _repository.GetById<Product>(productId);
            if (product is null)
            {
                _logger.LogInformation("Producto con ID {ProductId} no encontrado.", item.ProductId);
                throw new ArgumentException($" No existe el Producto con id: {item.ProductId}.");
            }
            if (!product.IsActive)
            {
                _logger.LogInformation("Producto con ID {ProductId} no está activo.", item.ProductId);
                throw new EntityNotActiveException($"No está activo el producto con id: {item.ProductId}.");
            }
            if (item.Quantity <= 0)
            {
                _logger.LogError("La cantidad del producto debe ser mayor a cero.");
                throw new ArgumentException("La cantidad del producto debe ser mayor a cero.");
            }
            if (product.StockQuantity < item.Quantity)
            {
                _logger.LogInformation("Stock insuficiente para el producto con ID {ProductId}.", item.ProductId);
                throw new InsufficientStockException($"Stock insuficiente para {product.Name}.");

            }
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
        {
            _logger.LogError("No se han agregado productos al pedido.");
            throw new ArgumentException("No se han agregado productos al pedido.");
        }

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

        _logger.LogInformation("Orden con ID {OrderId} creada exitosamente para el cliente {CustomerId}.", order.Id, request.CustomerId);

        return new OrderModel.OrderResponse(
            order.Id,
            order.Date,
            order.CustomerId,
            customer.Name!, 
            order.ShippingAddress,
            order.BillingAddress,
            order.Notes,
            order.Status.ToString(),
            order.TotalAmount,
            responseItems
        );
    }

    public async Task<OrderModel.PaginationResponse> GetOrders(OrderModel.FilterOrder request)
    {
        _logger.LogInformation(
            "Obteniendo órdenes con filtros - Estado: {Status}, ClienteNombre: {CustomerName}, Página: {PageNumber}, Tamaño de página: {PageSize}",
            request.Status,
            request.CustomerName,
            request.PageNumber,
            request.PageSize
        );

        OrderStatus? status = null;

        if (!string.IsNullOrWhiteSpace(request.Status) &&
            !string.Equals(request.Status, "all", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(request.Status, "todos", StringComparison.OrdinalIgnoreCase))
        {
            if (!Enum.TryParse<OrderStatus>(request.Status, true, out var parsedStatus))
            {
                parsedStatus = default;
            }
            status = parsedStatus;
        }

        var customerName = request.CustomerName?.Trim().ToLower();

        var query = await _repository.GetFiltered<Order>(
            o =>
                (status == null || o.Status == status) &&
                (string.IsNullOrEmpty(customerName) ||
                 o.Customer != null &&
                 o.Customer.Name != null &&
                 o.Customer.Name.ToLower().Contains(customerName)),
            "OrderItems.Product"
        );

        var total = query!.Count();

        var pageNumber = request.PageNumber ?? 1;
        var pageSize = request.PageSize ?? total;

        var orders = query!
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var orderResponses = new List<OrderModel.OrderResponse>();

        foreach (var o in orders)
        {
            var orderItems = o.OrderItems!.Select(i => new OrderModel.OrderItemResponse(
                i.ProductId,
                i.Quantity,
                i.UnitPrice,
                i.SubTotal
            )).ToList();

            var totalAmount = orderItems.Sum(i => i.SubTotal);

            var customer = await _repository.GetById<Customer>(o.CustomerId);
            var customerNameResult = customer?.Name ?? "Desconocido";

            orderResponses.Add(new OrderModel.OrderResponse(
                o.Id,
                o.Date,
                o.CustomerId,
                customerNameResult,
                o.ShippingAddress!,
                o.BillingAddress!,
                o.Notes ?? string.Empty,
                o.Status.ToString(),
                totalAmount,
                orderItems
            ));
        }

        return new OrderModel.PaginationResponse(orderResponses, total);
    }


    public async Task<OrderModel.OrderResponse> GetOrderById(Guid id)
    {
        _logger.LogInformation("Obteniendo orden con ID {OrderId}", id);
        var order = await _repository.GetById<Order>(id, "OrderItems.Product");
        if (order is null)
        {
            _logger.LogError("Orden con ID {OrderId} no encontrada.", id);
            throw new EntityNotFoundException($"No existe una orden con el ID {id}.");
        }

        var orderItems = order.OrderItems!.Select(i => new OrderModel.OrderItemResponse(
            i.ProductId,
            i.Quantity,
            i.UnitPrice,
            i.SubTotal
            )).ToList();

        var customerName = await _repository.GetById<Customer>(order.CustomerId) is Customer customer
            ? customer.Name
            : "Desconocido";

        return new OrderModel.OrderResponse(
            order.Id,
            order.Date,
            order.CustomerId,
            customerName!,
            order.ShippingAddress!,
            order.BillingAddress!,
            order.Notes!,
            order.Status.ToString(),
            order.TotalAmount,
            orderItems
        );
    }

    public async Task<OrderModel.OrderResponse> UpdateOrderStatus(Guid id, string newStatus)
    {
        _logger.LogInformation("Actualizando estado de la orden con ID {OrderId} a {NewStatus}", id, newStatus);
        var order = await _repository.GetById<Order>(id);
        if (order is null)
        {
            _logger.LogError("Orden con ID {OrderId} no encontrada.", id);
            throw new EntityNotFoundException($"No existe una orden con el ID {id}.");
        }

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
        {
            _logger.LogError("La orden ya se encuentra en el estado {StatusOld}.", statusOld);
            throw new ArgumentException($"la orden ya se encuentra en {statusOld}");
        }
        await _repository.Update(order);

        _logger.LogInformation("Estado de la orden con ID {OrderId} actualizado exitosamente a {NewStatus}", id, newStatus);

        return await GetOrderById(order.Id);
    }

}
