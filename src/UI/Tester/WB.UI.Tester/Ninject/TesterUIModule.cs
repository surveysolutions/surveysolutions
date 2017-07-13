using Ninject.Modules;
using WB.Core.BoundedContexts.Tester.Implementation.Services;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.UI.Shared.Enumerator.Services.Internals.MapService;
using WB.UI.Tester.Implementation.Services;
using WB.UI.Tester.Infrastructure.Internals.Log;

namespace WB.UI.Tester.Ninject
{
    public class TesterUIModule : NinjectModule
    {
        public override void Load()
        {
            this.Bind<IViewModelNavigationService>().To<ViewModelNavigationService>();

            this.Bind<IMapSynchronizer>().To<TesterMapSynchronizer>();
            this.Bind<IMapService>().To<MapService>();
            this.Bind<ILoggerProvider>().To<XamarinInsightsLoggerProvider>();

#if EXCLUDEEXTENSIONS
            this.Bind<IAreaEditService>().To<WB.UI.Shared.Enumerator.CustomServices.AreaEditor.DummyAreaEditService>();
#else
            this.Bind<IAreaEditService>().To<WB.UI.Shared.Extensions.CustomServices.AreaEditor.AreaEditService>();
#endif
        }
    }
}