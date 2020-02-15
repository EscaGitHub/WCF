using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Swr.Capital1C.Service.Domain.Services.Boms.Exceptions
{
    public class BomNotFoundException : Exception
    {
        public BomNotFoundException()
        {
            
        }

        public BomNotFoundException(string message) : base(message)
        {
            
        }
    }
}
