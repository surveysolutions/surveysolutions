using System;
using System.Collections.Generic;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Implementation;

namespace WB.UI.Headquarters.Services.Impl
{
    public class DesignerUserCredentials : IDesignerUserCredentials
    {
        private readonly IAuthorizedUser authorizedUser;
        private static readonly Dictionary<string, RestCredentials> credentialsStorage = new Dictionary<string, RestCredentials>();

        public DesignerUserCredentials() { }

        public DesignerUserCredentials(IAuthorizedUser authorizedUser)
        {
            this.authorizedUser = authorizedUser ?? throw new ArgumentNullException(nameof(authorizedUser));
        }

        public virtual RestCredentials Get()
        {
            return credentialsStorage.GetOrNull(this.authorizedUser.UserName);
        }

        public void Set(RestCredentials credentials)
            => credentialsStorage[this.authorizedUser.UserName] = credentials;
    }
}
