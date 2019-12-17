using System;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

namespace WB.UI.Shared.Web.Authentication
{
    public static class BasicAuthenticationExtensions
    {
        public static BasicCredentials ParseBasicCredentials(this IHeaderDictionary headerDictionary)
        {
            AuthenticationHeaderValue authHeader = AuthenticationHeaderValue.Parse(headerDictionary[HeaderNames.Authorization]);
            var credentialBytes = Convert.FromBase64String(authHeader.Parameter);
            var credentials = Encoding.UTF8.GetString(credentialBytes).Split(':');
            var username = credentials[0];
            var password = credentials[1];
            BasicCredentials creds = new BasicCredentials
            {
                Username = username, 
                Password = password
            };

            return creds;
        }
    }
}
