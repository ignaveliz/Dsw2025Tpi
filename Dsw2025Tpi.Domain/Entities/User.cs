using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dsw2025Tpi.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace Dsw2025Tpi.Domain.Entities;

public class User : IdentityUser, IEntityBase
{
   Guid IEntityBase.Id { get; set; }
   public ICollection<Order>? Orders { get; set; }

}
