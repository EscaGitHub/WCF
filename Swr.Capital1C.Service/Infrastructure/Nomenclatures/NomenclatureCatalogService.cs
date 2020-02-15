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
using Swr.Capital1C.Service.Domain.Services.Nomenclatures;
using Swr.Capital1C.Service.Domain.Services.Nomenclatures.Exceptions;
using Swr.Capital1C.Service.Domain.Services.Nomenclatures.Models.In;
using Swr.Capital1C.Service.Settings.Model;

namespace Swr.Capital1C.Service.Infrastructure.Nomenclatures
{
    public class NomenclatureCatalogService : INomenclatureCatalogService, IDisposable
    {
        private readonly CatalogServiceConnection _settings;
        private readonly HttpClient _client;

        public NomenclatureCatalogService(CatalogServiceConnection settings)
        {
            _settings = settings;
            _client = new HttpClient();
        }

        public async Task<Nomenclature> GetNomenclatureAsync(string id)
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
            return JsonConvert.DeserializeObject<Nomenclature>(content);
        }

        private static string Encode(string plainText)
        {
            var bytes = Encoding.UTF8.GetBytes(plainText);

            char[] padding = { '=' };

            return Convert.ToBase64String(bytes)
                .TrimEnd(padding).Replace('+', '-').Replace('/', '_');
        }

        public async Task UpdateNomenclatureAsync(Nomenclature nomenclature)
        {
            await Authorize();

            var requestUri = _settings.Address + Encode(nomenclature.Id);

            var request = new HttpRequestMessage(new HttpMethod("PATCH"), requestUri)
            {
                Content = new StringContent(GetContent(nomenclature), Encoding.UTF8, "application/json")
            };

            var response = await _client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                switch (response.StatusCode)
                {
                    case HttpStatusCode.NotFound:
                        throw new  NomenclatureNotFoundException($"Номенклатура не найдена по идентификатору '{nomenclature.Id}'");
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

        private static string GetContent(Nomenclature nomenclature)
        {
            return JsonConvert.SerializeObject(new { nomenclature.Attributes });
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
                case ErrorCode.NomenclatureIsInvalid:
                    throw new NomenclatureIsInvalidException(error.Message);
                case ErrorCode.NomenclatureNotSatisfyDefinition:
                    throw new NomenclatureIsNotSatisfyDefinitionException(error.Message);
            }
        }

        public class Error
        {
            public ErrorCode Code { get; set; }

            public string Message { get; set; }
        }

        public enum ErrorCode
        {
            NomenclatureNotSatisfyDefinition,
            NomenclatureIsInvalid
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

        public void Dispose()
        {
            _client?.Dispose();
        }
    }
}
