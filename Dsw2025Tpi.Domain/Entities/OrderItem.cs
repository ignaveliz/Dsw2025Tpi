using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dsw2025Tpi.Domain.Entities;

public class OrderItem
{
    int Quantity { get; set; }
    decimal UnitPrice { get; set; }
    decimal SubTotal { get; set; }
}
