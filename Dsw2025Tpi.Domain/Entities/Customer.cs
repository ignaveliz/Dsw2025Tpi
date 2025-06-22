using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dsw2025Tpi.Domain.Entities;

public class Customer : EntityBase
{
    string Email { get; set; }
    string Name { get; set; }
    string PhoneNumber { get; set; }
}
