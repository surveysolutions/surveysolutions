﻿using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.Modularity;

namespace WB.UI.Shared.Web.Modules
{
    public interface IWebModule : IInitModule
    {
        void Load(IWebIocRegistry registry);
    }
}
