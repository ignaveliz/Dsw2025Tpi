using Dsw2025Tpi.Domain.Entities;
using System;
using System.Threading.Tasks;

namespace Dsw2025Tpi.Application.Dtos
{
    public class UpdateOrderStatusRequest
    {
        public OrderStatus NewStatus { get; set; }
    }
}
