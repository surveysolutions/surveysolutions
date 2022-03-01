using System.Threading.Tasks;
using WB.Core.BoundedContexts.Tester.Implementation.Services;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.Modularity;
using WB.Core.SharedKernels.DataCollection.Implementation.Services;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.MapService;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups;
using WB.UI.Shared.Enumerator.Services;
using WB.UI.Tester.Implementation.Services;
using WB.UI.Tester.Infrastructure.Internals.Settings;

namespace WB.UI.Tester.ServiceLocation
{
    public class TesterUIModule : IModule
    {
        public void Load(IIocRegistry registry)
        {
            registry.Bind<TesterMvxApplication>();
            registry.Bind<TesterAppStart>();

            registry.Bind<ISideBarSectionViewModelsFactory, SideBarSectionViewModelFactory>();
            registry.Bind<IViewModelNavigationService, ViewModelNavigationService>();
            registry.Bind<IMapService, MapService>();
            registry.Bind<TesterSettings>();
            registry.Bind<PhotoViewViewModel>();
            registry.BindAsSingleton<IInterviewViewModelFactory, TesterInterviewViewModelFactory>();
            registry.Bind<IGroupStateCalculationStrategy, EnumeratorGroupGroupStateCalculationStrategy>();
            registry.Bind<IInterviewStateCalculationStrategy, EnumeratorInterviewStateCalculationStrategy>();
            registry.BindAsSingleton<IDenormalizerRegistry, TesterDenormalizerRegistry>();

#if EXCLUDEEXTENSIONS
            registry.Bind<IMapInteractionService, WB.UI.Shared.Enumerator.CustomServices.AreaEditor.DummyMapInteractionService>();
#else
            registry.Bind<WB.UI.Shared.Extensions.ViewModels.GeographyEditorViewModel>();
            registry.Bind<IMapInteractionService, WB.UI.Shared.Extensions.Services.MapInteractionService>();
            registry.Bind<WB.UI.Shared.Extensions.Services.IMapUtilityService, WB.UI.Shared.Extensions.Services.MapUtilityService>();
#endif
        }

        public Task Init(IServiceLocator serviceLocator, UnderConstructionInfo info)
        {
            return Task.CompletedTask;
        }
    }
}
