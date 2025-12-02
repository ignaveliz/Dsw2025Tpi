using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dsw2025Tpi.Application.Exceptions;

public class DuplicatedEntityException : ApplicationException 
{
    public DuplicatedEntityException(string message, string code = "") : base(message)
    {
        Code = code;
    }

    public string Code { get; set; }
}
