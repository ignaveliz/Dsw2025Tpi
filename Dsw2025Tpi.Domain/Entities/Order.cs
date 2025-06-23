using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dsw2025Tpi.Domain.Entities;

public class Order : EntityBase
{
    public DateTime Date { get; set; }
    public string ShippingAddress { get; set; }
    public string BillingAddress { get; set; }
    public string Notes { get; set; }
    public OrderStatus Status { get; set; }
    public decimal TotalAmount { get; set; }

    public Guid CustomerId { get; set; }
    public Customer? Customer { get; set; }

    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

}
