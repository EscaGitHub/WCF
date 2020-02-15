using Swr.Capital1C.Service.Domain.Services.Nomenclatures.Models.In;

namespace Swr.Capital1C.Service.Domain.Services.Nomenclatures
{
    public interface INomenclatureValidation
    {
        void Run(Nomenclature nomenclature, string codeAttributeName);
    }
}
