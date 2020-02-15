using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Swr.Capital1C.Service.Domain.Services.Boms.Exceptions
{
    public class BomIsInvalidException : Exception
    {
        public BomIsInvalidException(string message) : base(message)
        {
        }
    }
}
