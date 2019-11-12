using Microsoft.AspNetCore.Authentication;

namespace WB.UI.Designer.Code.Attributes
{
    public class BasicAuthenticationSchemeOptions : AuthenticationSchemeOptions
    {
        public string Realm { get; set; }
    }
}
