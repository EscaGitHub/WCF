using System;
using System.Collections.Generic;
using Swr.Capital1C.Service.Domain.Services.Nomenclatures;
using Swr.Capital1C.Service.Logger;
using Swr.Capital1C.Service.Settings.Model;

namespace Swr.Capital1C.Service.Infrastructure
{
    public static class ChangedDocumentsFiltering
    {
        public static List<ChangedDocument> Filter(List<ChangedDocument> currentDocuments)
        {
            var logger = LogHelper.Instance.GetLogger("Фильтрация");

            var result = new List<ChangedDocument>();

            foreach (var currentDocument in currentDocuments)
            {
                if (currentDocument.Version == 0 ||
                    string.Equals(currentDocument.ConfigurationName, "@", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(currentDocument.IsService, "1", StringComparison.OrdinalIgnoreCase))
                {
                    logger.Debug($"Документ с артикулом '{currentDocument.Article}' и идентификатором '{currentDocument.GetUniqueId()}' отфильтрован");
                    continue;
                }

                result.Add(currentDocument);
            }

            logger.Debug($"Количество документов после фильтрации '{result.Count}'");

            return result;
        }
    }
}
