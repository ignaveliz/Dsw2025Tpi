using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dsw2025Tpi.Domain.Entities;

namespace Dsw2025Tpi.Application.Dtos;

public record OrderModel
{
    public record OrderItemRequest(Guid ProductId,int Quantity,string? Name,string? Description,decimal UnitPrice);
    public record OrderItemResponse(Guid ProductId, int Quantity, decimal UnitPrice, decimal SubTotal);

    public record OrderRequest(Guid CustomerId, string? ShippingAddress, string? BillingAddress, string? Notes, ICollection<OrderItemRequest> OrderItems);
    public record OrderResponse(Guid OrderId, DateTime Date, Guid CustomerId, string ShippingAdress, string BillingAdress, string Notes, OrderStatus Status, decimal TotalAmount, ICollection<OrderItemResponse> OrderItems);

    public record UpdateOrderStatusRequest(string? NewStatus);

}
