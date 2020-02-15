namespace Swr.Capital1C.Service.Domain.Services.Nomenclatures
{
    public class CatalogItemId
    {
        public CatalogItemId(int fileId, string configuration, int version)
        {
            FileId = fileId;
            Version = version;
            Configuration = configuration;
        }

        public int FileId { get; }

        public int Version { get; }

        public string Configuration { get; }

        public override string ToString()
        {
            return $"{FileId}:{Configuration}:{Version}";
        }
    }
}