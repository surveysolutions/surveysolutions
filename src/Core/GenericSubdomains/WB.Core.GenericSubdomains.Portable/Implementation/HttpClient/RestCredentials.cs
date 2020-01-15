using System;
using System.Net.Http.Headers;
using System.Text;

namespace WB.Core.GenericSubdomains.Portable.Implementation
{
    public class RestCredentials
    {
        public string Login { get; set; }
        public string Password { get; set; }
        public string Token { get; set; }

        public AuthenticationHeaderValue GetAuthenticationHeaderValue()
        {
            var header = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{Login}:{Password}"));
            return new AuthenticationHeaderValue("Basic", header);
        }
    }
}
