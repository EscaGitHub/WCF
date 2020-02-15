using System;

namespace Swr.Capital1C.Service.Domain.Services.Nomenclatures.Exceptions
{
    public class NomenclatureNotFoundException : Exception
    {
        public NomenclatureNotFoundException()
        {
            
        }

        public NomenclatureNotFoundException(string message) : base(message)
        {
        }
    }
}
