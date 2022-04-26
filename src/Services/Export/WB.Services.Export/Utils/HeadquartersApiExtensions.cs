using System;
using System.Net.Http;
using WB.Services.Export.Services;

namespace WB.Services.Export
{
    public static class HeadquartersApiExtensions
    {
        public static string? BaseUrl(this IHeadquartersApi api)
        {
            var apiType = api.GetType();
            var httpClientProperty = apiType.GetProperty("Client");
            if (httpClientProperty != null)
            {
                if (httpClientProperty.GetValue(api) is HttpClient httpClient)
                {
                    return httpClient.BaseAddress?.ToString();
                }
            }

            throw new ArgumentException("Cannot get Refit HttpClient from IHeadquartersApi");
        }
    }
}
