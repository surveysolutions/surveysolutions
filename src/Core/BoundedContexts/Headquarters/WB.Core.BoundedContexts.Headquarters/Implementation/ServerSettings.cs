using System;
using WB.Core.BoundedContexts.Headquarters.Views;

namespace WB.Core.BoundedContexts.Headquarters.Implementation
{
    public class ServerSettings
    {
        public virtual string Id { get; set; }
        
        public virtual string Value { get; set; }

        public const string PublicTenantIdKey = "TenantPublicId";
    }
}
