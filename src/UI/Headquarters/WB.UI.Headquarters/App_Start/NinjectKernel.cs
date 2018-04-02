using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Ninject;
using Ninject;
using Ninject.Web.Common;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.Modularity;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Binding;
using WB.Infrastructure.Native.Ioc;

using IKernel = Ninject.IKernel;

namespace WB.UI.Shared.Web.Modules
{
    public class NinjectKernel : WB.Core.Infrastructure.Modularity.IKernel
    {
        public NinjectKernel()
        {
            var ninjectSettings = new NinjectSettings { InjectNonPublic = true };
            this.Kernel = new StandardKernel(ninjectSettings);

            // ServiceLocator
            ServiceLocator.SetLocatorProvider(() => new NativeNinjectServiceLocatorAdapter(this.Kernel));
            this.Kernel.Bind<IServiceLocator>().ToConstant(ServiceLocator.Current);
        }

        public IKernel Kernel { get; set; }
        private readonly List<IInitModule> initModules = new List<IInitModule>();

        public void Load(params IModule[] modules)
        {
            var ninjectModules = modules.Select(module => module.AsNinject()).ToArray();
            this.Kernel.Load(ninjectModules);
            initModules.AddRange(modules.Select(m => m as IInitModule).Where(m => m != null));
        }

        public void Load(params IWebModule[] modules)
        {
            var ninjectModules = modules.Select(module => module.AsWebNinject()).ToArray();
            this.Kernel.Load(ninjectModules);
            initModules.AddRange(modules.Select(m => m as IInitModule).Where(m => m != null));
        }

        public void Init()
        {
            this.Kernel.Bind<Func<IKernel>>().ToMethod(ctx => () => new Bootstrapper().Kernel);
            this.Kernel.Bind<IHttpModule>().To<HttpApplicationInitializationHttpModule>();

            GlobalHost.DependencyResolver = new NinjectDependencyResolver(this.Kernel);
            ModelBinders.Binders.DefaultBinder = new GenericBinderResolver(this.Kernel);

            initModules.ForEach(m => m.Init(ServiceLocator.Current));
        }
    }
}