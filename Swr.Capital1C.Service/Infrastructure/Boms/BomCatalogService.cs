using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using Swr.Capital1C.Service.Domain.Services.Boms;
using Swr.Capital1C.Service.Domain.Services.Boms.Exceptions;
using Swr.Capital1C.Service.Domain.Services.Boms.Models.In;
using Swr.Capital1C.Service.Domain.Services.Nomenclatures.Models.In;
using Swr.Capital1C.Service.Settings.Model;

namespace Swr.Capital1C.Service.Infrastructure.Boms
{
    public class BomCatalogService : IBomCatalogService, IDisposable
    {
        private readonly HttpClient _client;
        private readonly CatalogServiceConnection _settings;

        public BomCatalogService(CatalogServiceConnection settings)
        {
            _settings = settings;
            _client = new HttpClient();
        }

        public async Task<Bom> GetBomAsync(string id)
        {
            await Authorize();

            var requestUri = _settings.Address + Encode(id);
            var httpResponse = await _client.GetAsync(requestUri);

            if (!httpResponse.IsSuccessStatusCode)
            {
                switch (httpResponse.StatusCode)
                {
                    case HttpStatusCode.NotFound:
                        return null;
                    case HttpStatusCode.BadRequest:
                        {
                            await HandleBadRequest(httpResponse);
                            break;
                        }
                    default:
                        throw new InvalidOperationException();
                }
            }

            var content = await httpResponse.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<Bom>(content);
        }

        public async Task UpdateBomAsync(Bom bom)
        {
            await Authorize();

            var requestUri = _settings.Address + Encode(bom.Id);

            var request = new HttpRequestMessage(new HttpMethod("PATCH"), requestUri)
            {
                Content = new StringContent(GetContent(bom), Encoding.UTF8, "application/json")
            };

            var response = await _client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                switch (response.StatusCode)
                {
                    case HttpStatusCode.NotFound:
                        throw new BomNotFoundException($"Спецификация не найдена по идентификатору '{bom.Id}'");
                    case HttpStatusCode.BadRequest:
                    {
                        await HandleBadRequest(response);
                        break;
                    }
                    default:
                        throw new InvalidOperationException();
                }
            }
        }

        private static string GetContent(Nomenclature bom)
        {
            return JsonConvert.SerializeObject(new { bom.Attributes });
        }

        private static string Encode(string plainText)
        {
            var bytes = Encoding.UTF8.GetBytes(plainText);

            char[] padding = { '=' };

            return Convert.ToBase64String(bytes)
                .TrimEnd(padding).Replace('+', '-').Replace('/', '_');
        }

        private async Task Authorize()
        {
            var tokenRequest = _settings.IdentityAddress + HttpUtility.UrlPathEncode($"?userName={_settings.Login}&password={_settings.Password}");

            var tokenResponse = await _client.GetAsync(tokenRequest);

            if (!tokenResponse.IsSuccessStatusCode)
            {
                switch (tokenResponse.StatusCode)
                {
                    case HttpStatusCode.BadRequest:
                        throw new AuthenticationException(await tokenResponse.Content.ReadAsStringAsync());
                    default:
                        throw new InvalidOperationException();
                }
            }

            var tokenContent = await tokenResponse.Content.ReadAsStringAsync();

            var token = JsonConvert.DeserializeObject<TokenResponse>(tokenContent);

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Access_token);
        }

        private class TokenResponse
        {
            public string Access_token { get; set; }

            public string Username { get; set; }
        }

        private static async Task HandleBadRequest(HttpResponseMessage httpResponse)
        {
            var content = await httpResponse.Content.ReadAsStringAsync();

            if (content != null)
                HandleBadRequestContent(content);

            throw new InvalidOperationException();
        }

        private static void HandleBadRequestContent(string content)
        {
            Error error;

            try
            {
                error = JsonConvert.DeserializeObject<Error>(content);
            }
            catch (Exception)
            {
                throw new InvalidOperationException(content);
            }

            switch (error.Code)
            {
                case ErrorCode.BomIsInvalid:
                    throw new BomIsInvalidException(error.Message);
                case ErrorCode.BomNotSatisfyDefinition:
                    throw new BomIsNotSatisfyDefinitionException(error.Message);
            }
        }

        public class Error
        {
            public ErrorCode Code { get; set; }

            public string Message { get; set; }
        }

        public enum ErrorCode
        {
            BomNotSatisfyDefinition,
            BomIsInvalid
        }

        public void Dispose()
        {
            _client?.Dispose();
        }
    }
}
