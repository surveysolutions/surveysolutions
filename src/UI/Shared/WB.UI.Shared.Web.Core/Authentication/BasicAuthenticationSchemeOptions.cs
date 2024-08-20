using Microsoft.AspNetCore.Authentication;

namespace WB.UI.Shared.Web.Authentication
{
    public class BasicAuthenticationSchemeOptions : AuthenticationSchemeOptions
    {
        public string? Realm { get; set; }
    }
}
