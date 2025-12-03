using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dsw2025Tpi.Application.Exceptions;

public class InvalidValueException : ApplicationException
{
    public string Code { get; set; }

    public InvalidValueException(string message,string code="") : base(message)
    {
        Code = code;
    }
}
