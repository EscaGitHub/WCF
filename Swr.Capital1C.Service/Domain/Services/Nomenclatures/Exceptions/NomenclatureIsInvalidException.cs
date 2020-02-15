using System;

namespace Swr.Capital1C.Service.Domain.Services.Nomenclatures.Exceptions
{
    public class NomenclatureIsInvalidException : Exception
    {
        public NomenclatureIsInvalidException()
        {

        }

        public NomenclatureIsInvalidException(string message) : base(message)
        {

        }
    }
}
