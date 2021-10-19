using System;
using Microsoft.IdentityModel.Tokens;

namespace WB.UI.Headquarters.Code.Authentication
{
    public class TokenProviderOptions
    {

        public bool IsBearerEnabled { set; get; }

        public string Issuer { get; set; }
        public string Audience { get; set; }
        public TimeSpan Expiration { get; set; } = TimeSpan.FromHours(12*365);
        public SigningCredentials SigningCredentials { get; set; }
    }
}
