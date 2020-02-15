using Swr.Capital1C.Okei;
using Swr.Infrastructure.Mailing;

namespace Swr.Capital1C.Service.Settings.Model
{
    public class CommonSettings : ICommonSettings
    {
        public CommonSettings()
        {
            NomenclatureCatalogServiceConnection = new CatalogServiceConnection();
            BomCatalogServiceConnection = new CatalogServiceConnection();
            PdmDbConnection = new DbConnection();
            ExportedDocumentsStatusDbConnection = new DbConnection();
            NomenclatureDefinition = new NomenclatureDefinition();
            BomDefinition = new BomDefinition();
            OkeiServiceConnection = new OkeiServiceConnection();
        }

        public string IsServiceVariableName { get; set; }

        public string ArticleVariableName { get; set; }

        public DbConnection PdmDbConnection { get; set; }

        public DbConnection ExportedDocumentsStatusDbConnection { get; set; }

        public CatalogServiceConnection NomenclatureCatalogServiceConnection { get; set; }

        public CatalogServiceConnection BomCatalogServiceConnection { get; set; }

        public NomenclatureDefinition NomenclatureDefinition { get; set; }

        public BomDefinition BomDefinition { get; set; }

        public OkeiServiceConnection OkeiServiceConnection { get; set; }

        public EmailServiceSettings EmailServiceSettings { get; set; }
    }
}