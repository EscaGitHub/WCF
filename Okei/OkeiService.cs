using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;

namespace Swr.Capital1C.Okei
{
    public class OkeiService : IOkeiService
    {
        private readonly string _baseUrl;

        public OkeiService(OkeiServiceConnection connection)
        { 
            _baseUrl = connection.Address;
        }

        public async Task<string> GetOkeiCodeAsync(string unitOfMeasure)
        {
            using (var client = new HttpClient())
            {
                var requestUri = BuildRequestUri(unitOfMeasure);

                using (var httpResponse = await client.GetAsync(requestUri))
                {
                    if (!httpResponse.IsSuccessStatusCode)
                        if (httpResponse.StatusCode == HttpStatusCode.NotFound)
                            return null;
                        else
                            throw new InvalidOperationException();

                    using (var httpContent = httpResponse.Content)
                    {
                        var content = await httpContent.ReadAsStringAsync();
                        return JsonConvert.DeserializeObject<OkeiRecord>(content)?.Code;
                    }
                }
            }
        }

        private string BuildRequestUri(string unitOfMeasure)
        {
            return _baseUrl + Encode(unitOfMeasure);
        }
        private static string Encode(string plainText)
        {
            var bytes = Encoding.UTF8.GetBytes(plainText);

            char[] padding = { '=' };

            return Convert.ToBase64String(bytes)
                .TrimEnd(padding).Replace('+', '-').Replace('/', '_');
        }
    }
}