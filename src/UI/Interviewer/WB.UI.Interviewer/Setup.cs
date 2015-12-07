using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Android.Content;
using Android.Widget;
using Cirrious.CrossCore;
using Cirrious.CrossCore.Converters;
using Cirrious.CrossCore.IoC;
using Cirrious.MvvmCross.Binding.Bindings.Target.Construction;
using Cirrious.MvvmCross.ViewModels;
using Cirrious.MvvmCross.Views;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ncqrs.Eventing.Storage;
using Ninject;
using WB.Core.BoundedContexts.Interviewer.Implementation.Services;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.EventBus.Hybrid.Implementation;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.Infrastructure.Ncqrs;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.Enumerator;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.SurveyManagement;
using WB.Infrastructure.Shared.Enumerator;
using WB.Infrastructure.Shared.Enumerator.Ninject;
using WB.UI.Interviewer.Activities;
using WB.UI.Interviewer.Converters;
using WB.UI.Interviewer.CustomBindings;
using WB.UI.Interviewer.Infrastructure;
using WB.UI.Interviewer.Ninject;
using WB.UI.Interviewer.Settings;
using WB.UI.Interviewer.ViewModel;
using WB.UI.Shared.Enumerator;
using WB.UI.Shared.Enumerator.Ninject;

namespace WB.UI.Interviewer
{
    public class Setup : EnumeratorSetup
    {
        public Setup(Context applicationContext) : base(applicationContext){}

        protected override void InitializeViewLookup()
        {
            base.InitializeViewLookup();

            var viewModelViewLookup = new Dictionary<Type, Type>()
            {
                {typeof(LoginViewModel), typeof(LoginActivity)},
                {typeof(FinishInstallationViewModel), typeof(FinishInstallationActivity)},
                {typeof(DashboardViewModel), typeof(DashboardActivity)},
                {typeof(TroubleshootingViewModel), typeof(TroubleshootingActivity)},
                {typeof(InterviewerInterviewViewModel), typeof(InterviewActivity)},
                {typeof(RelinkDeviceViewModel), typeof(RelinkDeviceActivity)}
            };

            var container = Mvx.Resolve<IMvxViewsContainer>();
            container.AddAll(viewModelViewLookup);
        }

        protected override IMvxApplication CreateApp()
        {
            return new App();
        }

        protected override void FillValueConverters(IMvxValueConverterRegistry registry)
        {
            base.FillValueConverters(registry);

            registry.AddOrOverwrite("Localization", new InterviewerLocalizationValueConverter());
            registry.AddOrOverwrite("InterviewStatusToColor", new InterviewStatusToColorConverter());
            registry.AddOrOverwrite("SynchronizationStatusToDrawable", new SynchronizationStatusToDrawableConverter());
            registry.AddOrOverwrite("ValidationStyleBackground", new TextEditValidationStyleBackgroundConverter());
            registry.AddOrOverwrite("IsSynchronizationFailOrCanceled", new IsSynchronizationFailOrCanceledConverter());
            registry.AddOrOverwrite("SynchronizationStatusToTextColor", new SynchronizationStatusToTextColorConverter());
        }

        protected override void FillTargetFactories(IMvxTargetBindingFactoryRegistry registry)
        {
            registry.RegisterCustomBindingFactory<TextView>("IsCurrentDashboardTab", (view) => new TextViewIsCurrentDashboardTabBinding(view));

            base.FillTargetFactories(registry);
        }


        protected override IMvxIoCProvider CreateIocProvider()
        {
            return new NinjectMvxIocProvider(this.CreateAndInitializeIoc());
        }

        private IKernel CreateAndInitializeIoc()
        {
            var kernel = new StandardKernel(

                new NcqrsModule().AsNinject(),
                new InfrastructureModuleMobile().AsNinject(),
                new InterviewerInfrastructureModule(),

                new EnumeratorSharedKernelModule(),
                new EnumeratorInfrastructureModule(),
                new EnumeratorUIModule(),
                new InterviewerUIModule(),
                
                new AndroidSharedModule());

            kernel.Load(
                new AndroidModelModule(),
                new AndroidDataCollectionSharedKernelModule());

            var liteEventBus = kernel.Get<ILiteEventBus>();
            kernel.Unbind<ILiteEventBus>();

            var cqrsEventBus = new InProcessEventBus(kernel.Get<IEventStore>(), new EventBusSettings(),
                kernel.Get<ILogger>());

            var hybridEventBus = new HybridEventBus(liteEventBus, cqrsEventBus);

            kernel.Bind<IEventBus>().ToConstant(hybridEventBus);
            kernel.Bind<ILiteEventBus>().ToConstant(hybridEventBus);

            kernel.Bind<IInterviewerSettings>().To<InterviewerSettings>();
            kernel.Bind<ISynchronizationService>().To<SynchronizationService>();

            kernel.Bind<ISyncProtocolVersionProvider>().To<SyncProtocolVersionProvider>().InSingletonScope();

            this.InitDashboard(kernel, cqrsEventBus);

            return kernel;
        }

        private void InitDashboard(IKernel kernel, InProcessEventBus bus)
        {
            var dashboardeventHandler = new InterviewEventHandler(
                kernel.Get<IAsyncPlainStorage<InterviewView>>(),
                kernel.Get<IAsyncPlainStorage<QuestionnaireDocumentView>>());

            bus.RegisterHandler(dashboardeventHandler, typeof(SynchronizationMetadataApplied));
            bus.RegisterHandler(dashboardeventHandler, typeof(InterviewStatusChanged));
            bus.RegisterHandler(dashboardeventHandler, typeof(InterviewSynchronized));
            bus.RegisterHandler(dashboardeventHandler, typeof(InterviewHardDeleted));

            bus.RegisterHandler(dashboardeventHandler, typeof(InterviewOnClientCreated));

            bus.RegisterHandler(dashboardeventHandler, typeof(TextQuestionAnswered));
            bus.RegisterHandler(dashboardeventHandler, typeof(MultipleOptionsQuestionAnswered));
            bus.RegisterHandler(dashboardeventHandler, typeof(SingleOptionQuestionAnswered));
            bus.RegisterHandler(dashboardeventHandler, typeof(NumericRealQuestionAnswered));
            bus.RegisterHandler(dashboardeventHandler, typeof(NumericIntegerQuestionAnswered));
            bus.RegisterHandler(dashboardeventHandler, typeof(DateTimeQuestionAnswered));

            bus.RegisterHandler(dashboardeventHandler, typeof(GeoLocationQuestionAnswered));
            bus.RegisterHandler(dashboardeventHandler, typeof(QRBarcodeQuestionAnswered));

            bus.RegisterHandler(dashboardeventHandler, typeof(AnswerRemoved));
        }

        protected override IList<Assembly> AndroidViewAssemblies
        {
            get
            {
                var toReturn = base.AndroidViewAssemblies;

                // Add assemblies with other views we use.  When the XML is inflated
                // MvvmCross knows about the types and won't complain about them.  This
                // speeds up inflation noticeably.
                return toReturn;
            }
        }

        protected override Assembly[] GetViewModelAssemblies()
        {
            return base.GetViewModelAssemblies().Union(new[]
            {
                typeof(AndroidCoreRegistry).Assembly,
                typeof(LoginViewModel).Assembly,

            }).ToArray();
        }
    }
}