using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Swr.Capital1C.Service.Domain.Services.Nomenclatures;
using Swr.Capital1C.Service.Infrastructure;
using Xunit;

namespace UnitTests
{
    public class ChangedDocumentsFilteringTest
    {
        [Fact]
        public void Filter_ShouldReturnDocuments()
        {
            List<ChangedDocument> currentDocuments = new List<ChangedDocument>
            {
                new ChangedDocument(1, "@", 2) {Article = "001"},
                new ChangedDocument(2, "-01", 2) {Article = "002"},
                new ChangedDocument(3, "По умолчанию", 2) {Article = "003"},
                new ChangedDocument(4, "-02", 0) {Article = "004"},
                new ChangedDocument(5, "-03", 0) {Article = "005", IsService = "1"},
            };

            var filteredDocuments = ChangedDocumentsFiltering.Filter(currentDocuments);

            Assert.Equal(2, filteredDocuments.Count);
            Assert.Equal("002", filteredDocuments[0].Article);
            Assert.Equal("003", filteredDocuments[1].Article);
        }
    }
}
