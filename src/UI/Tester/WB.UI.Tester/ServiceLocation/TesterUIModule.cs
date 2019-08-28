﻿using System.Threading.Tasks;
using WB.Core.BoundedContexts.Tester.Implementation.Services;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.Infrastructure.Modularity;
using WB.Core.SharedKernels.DataCollection.Implementation.Services;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.Enumerator.Denormalizer;
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

#if EXCLUDEEXTENSIONS
            registry.Bind<IAreaEditService, WB.UI.Shared.Enumerator.CustomServices.AreaEditor.DummyAreaEditService>();
#else
            registry.Bind<WB.UI.Shared.Extensions.CustomServices.AreaEditor.AreaEditorViewModel>();
            registry.Bind<IAreaEditService, WB.UI.Shared.Extensions.CustomServices.AreaEditor.AreaEditService>();
#endif
        }

        public Task Init(IServiceLocator serviceLocator, UnderConstructionInfo info)
        {
#if !EXCLUDEEXTENSIONS
            WB.UI.Shared.Extensions.CustomServices.AreaEditor.AreaEditService.RegisterLicense();
#endif
            return Task.CompletedTask;
        }
    }
}
