﻿using Ninject.Modules;
using WB.Core.GenericSubdomains.Android.Rest;
using WB.Core.GenericSubdomains.Portable.Rest;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.UI.Interviewer.Implementations.Services;

namespace WB.UI.Interviewer
{
    public class AndroidSharedModule : NinjectModule
    {
        public override void Load()
        {
            this.Bind<INetworkService>().To<AndroidNetworkService>().InSingletonScope();
            this.Bind<IRestServicePointManager>().To<RestServicePointManager>().InSingletonScope();
            this.Bind<IRestClientProvider>().To<FlurlRestClientProvider>().InSingletonScope();
            this.Bind<IRestService>().To<RestService>().InSingletonScope();
        }
    }
}
