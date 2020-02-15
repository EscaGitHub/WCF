using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Swr.Capital1C.Okei;
using Xunit;

namespace IntegrationTest
{
    public class OkeiServiceTest
    {
        private const string Skip = "Only for manual test";

        [Fact(Skip = "")]
        public async void GetOkeiCodeAsync_ShouldReturnSuccessCodeAndResponseWithCode()
        {
            var service = new OkeiService(new OkeiServiceConnection
            {
                Address = "http://localhost:5002/api/v2/okei/national-legends/"
            });

            var code = await service.GetOkeiCodeAsync("10^3 м");

            Assert.Equal("8", code);
        }
    }
}
