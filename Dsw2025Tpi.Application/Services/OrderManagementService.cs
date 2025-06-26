using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dsw2025Tpi.Application.Dtos;
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

    public async Task<OrderModel.Response?> CreateOrder(OrderModel.Request request)
    {
        var customer = await _repository.GetById<Customer>(request.CustomerId);
        if (customer == null) return null;// aca deberia tirar una excepcion personalizada

        var orderItems = new List<OrderItem>();
        decimal totalAmount = 0;

        foreach (var item in request.OrderItems)
        {
            var product = await _repository.GetById<Product>(item.ProductId);
            if (product is null || !product.IsActive || product.StockQuantity < item.Quantity)
                return null;

            var subTotal = product.CurrentUnitPrice;
            totalAmount = subTotal * item.Quantity;

            product.StockQuantity -= item.Quantity;
            await _repository.Update(product);

            orderItems.Add(new OrderItem
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                SubTotal = subTotal
            });

            totalAmount += subTotal;
        }

        var order = new Order
        {
            CustomerId = request.CustomerId,
            ShippingAddress = request.ShippingAdress,
            BillingAddress = request.BillingAdress,
            Date = DateTime.UtcNow,
            Status = OrderStatus.Pending,
            TotalAmount = totalAmount,
            OrderItems = orderItems
        };

        await _repository.Add(order);

        return new OrderModel.Response(
            order.Id,
            order.Date,
            order.ShippingAddress,
            order.BillingAddress,
            order.Notes,
            order.Status,
            order.TotalAmount,
            order.OrderItems.Select(i => new OrderModel.OrderItemResponse(
                i.ProductId,
                i.Quantity,
                i.UnitPrice,
                i.SubTotal
                )).ToList());
       
    }
}
