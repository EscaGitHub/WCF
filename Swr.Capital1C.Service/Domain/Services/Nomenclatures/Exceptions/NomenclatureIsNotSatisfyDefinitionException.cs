using System;

namespace Swr.Capital1C.Service.Domain.Services.Nomenclatures.Exceptions
{
    public class NomenclatureIsNotSatisfyDefinitionException : Exception
    {
        public NomenclatureIsNotSatisfyDefinitionException()
        {

        }

        public NomenclatureIsNotSatisfyDefinitionException(string message) : base(message)
        {

        }
    }
}
