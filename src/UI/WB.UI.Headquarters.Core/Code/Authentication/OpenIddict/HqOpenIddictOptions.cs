using System;

namespace WB.UI.Headquarters.Code.Authentication.OpenIddict
{
    public class HqOpenIddictOptions
    {
        public bool Enabled { get; set; }
        public string Issuer { get; set; }
        public TimeSpan AuthorizationCodeLifetime { get; set; } = TimeSpan.FromMinutes(5);
        public TimeSpan IdentityTokenLifetime { get; set; } = TimeSpan.FromMinutes(5);
        public TimeSpan AccessTokenLifetime { get; set; } = TimeSpan.FromMinutes(15);
        public bool RequirePkce { get; set; } = true;
        public string SigningCertificatePath { get; set; }
        public string SigningCertificatePassword { get; set; }
    }
}
