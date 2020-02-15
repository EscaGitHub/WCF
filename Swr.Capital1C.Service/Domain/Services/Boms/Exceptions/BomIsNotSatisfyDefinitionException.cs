using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Swr.Capital1C.Service.Domain.Services.Boms.Exceptions
{
    public class BomIsNotSatisfyDefinitionException : Exception
    {
        public BomIsNotSatisfyDefinitionException()
        {

        }

        public BomIsNotSatisfyDefinitionException(string message) : base(message)
        {
        }
    }
}
