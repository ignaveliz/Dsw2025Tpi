using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dsw2025Tpi.Domain.Entities;

public class Order : EntityBase
{
    DateTime Date { get; set; }
    string ShippingAddress { get; set; }
    string BillingAddress { get; set; }
    string Notes { get; set; }
    OrderStatus Status { get; set; }
    decimal TotalAmount { get; set; }

}
