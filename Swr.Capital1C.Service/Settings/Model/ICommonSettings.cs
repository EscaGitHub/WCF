using Swr.Capital1C.Okei;
using Swr.Infrastructure.Mailing;

namespace Swr.Capital1C.Service.Settings.Model
{
    public interface ICommonSettings
    {
        string IsServiceVariableName { get; set; }

        string ArticleVariableName { get; set; }

        DbConnection PdmDbConnection { get; set; }

        DbConnection ExportedDocumentsStatusDbConnection { get; set; }

        CatalogServiceConnection NomenclatureCatalogServiceConnection { get; set; }

        CatalogServiceConnection BomCatalogServiceConnection { get; set; }

        NomenclatureDefinition NomenclatureDefinition { get; set; }

        BomDefinition BomDefinition { get; set; }

        OkeiServiceConnection OkeiServiceConnection { get; set; }

        EmailServiceSettings EmailServiceSettings { get; set; }
    }
}