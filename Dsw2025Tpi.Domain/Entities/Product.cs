using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dsw2025Tpi.Domain.Entities;

public class Product : EntityBase
{
    public Product()
    {
        
    }

    public Product(string sku,string internalCode, string name, string description, decimal currentUnitPrice, int stockQuantity)
    {
        Sku = sku;
        InternalCode = internalCode;
        Name = name;
        Description = description;
        CurrentUnitPrice = currentUnitPrice;
        StockQuantity = stockQuantity;
        IsActive = true;

    }

    string? Sku { get; set; }
    string? InternalCode { get; set; }
    string? Name { get; set; }
    string? Description { get; set; }
    decimal CurrentUnitPrice { get; set; }
    int StockQuantity { get; set; }
    bool IsActive { get; set; }

}
