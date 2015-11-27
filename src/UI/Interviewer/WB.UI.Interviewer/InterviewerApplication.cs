using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Android.App;
using Android.Content;
using Android.Runtime;
using Cirrious.CrossCore.Core;
using Cirrious.MvvmCross.Droid.Platform;
using Main.Core.Events.File;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ncqrs.Eventing.Storage;
using Ninject;
using WB.Core.BoundedContexts.Interviewer.ErrorReporting;
using WB.Core.BoundedContexts.Interviewer.Implementation.Services;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.Infrastructure;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.EventBus.Hybrid.Implementation;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.Infrastructure.Ncqrs;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Core.SharedKernels.Enumerator;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.SurveyManagement;
using WB.Infrastructure.Shared.Enumerator;
using WB.Infrastructure.Shared.Enumerator.Ninject;
using WB.UI.Interviewer.Activities;
using WB.UI.Interviewer.Backup;
using WB.UI.Interviewer.EventHandlers;
using WB.UI.Interviewer.Infrastructure;
using WB.UI.Interviewer.Infrastructure.Internals.Crasher;
using WB.UI.Interviewer.Infrastructure.Internals.Crasher.Attributes;
using WB.UI.Interviewer.Infrastructure.Logging;
using WB.UI.Interviewer.Ninject;
using WB.UI.Interviewer.Settings;
using WB.UI.Shared.Enumerator;
using IInfoFileSupplierRegistry = WB.Core.GenericSubdomains.Portable.Services.IInfoFileSupplierRegistry;
using ILogger = WB.Core.GenericSubdomains.Portable.Services.ILogger;

namespace WB.UI.Interviewer
{
#if DEBUG 
    [Application(Debuggable=true)] 
#else
    [Application(Debuggable = false)]
#endif
    [Crasher(UseCustomData = false)]
    public class InterviewerApplication : Application
    {
        public static IKernel Kernel
        {
            get
            {
                if (Context == null)
                    return null;
                var capiApp = Context.ApplicationContext as InterviewerApplication;
                if (capiApp == null)
                    return null;
                return capiApp.kernel;
            }
        }

        protected InterviewerApplication(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
        }

        private void InitFileStorage(InProcessEventBus bus)
        {
            var fileSorage = new AndroidFileStoreDenormalizer(this.kernel.Get<IReadSideRepositoryWriter<FileDescription>>());
            bus.RegisterHandler(fileSorage, typeof (FileUploaded));
            bus.RegisterHandler(fileSorage, typeof (FileDeleted));
        }

        private void InitDashboard(InProcessEventBus bus)
        {
            var dashboardeventHandler = new InterviewEventHandler(
                this.kernel.Get<IAsyncPlainStorage<InterviewView>>(),
                this.kernel.Get<IAsyncPlainStorage<QuestionnaireDocumentView>>());

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

        internal static void RegisterEventHandlers(InProcessEventBus bus, IKernel kernel)
        {
            IEnumerable<object> handlers = Enumerable.Distinct<object>(kernel.GetAll(typeof(IEventHandler<>))).ToList();
            foreach (object handler in handlers)
            {
                IEnumerable<Type> ieventHandlers = handler.GetType().GetInterfaces().Where(IsIEventHandlerInterface);
                foreach (Type ieventHandler in ieventHandlers)
                {
                    bus.RegisterHandler(handler, ieventHandler.GenericTypeArguments[0]);
                }
            }
        }

        private static bool IsIEventHandlerInterface(Type type)
        {
            var typeInfo = type.GetTypeInfo();
            return typeInfo.IsInterface && typeInfo.IsGenericType && typeInfo.GetGenericTypeDefinition() == typeof(IEventHandler<>);
        }

        public override void OnCreate()
        {
            base.OnCreate();

            this.RestoreAppState();

            var basePath = Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.Personal))
                ? Environment.GetFolderPath(Environment.SpecialFolder.Personal)
                : Android.OS.Environment.ExternalStorageDirectory.AbsolutePath;
            
            const string QuestionnaireAssembliesFolder = "assemblies";

            this.kernel = new StandardKernel(

                new NcqrsModule().AsNinject(),
                new InfrastructureModuleMobile().AsNinject(),
                new DataCollectionInfrastructureModule(basePath).AsNinject(),

                new InterviewerInfrastructureModule(QuestionnaireAssembliesFolder),

                new EnumeratorSharedKernelModule(),
                new EnumeratorInfrastructureModule(),
                new EnumeratorUIModule(),
                new InterviewerUIModule(),
                
                new AndroidCoreRegistry(),
                new AndroidSharedModule(),
                new AndroidLoggingModule());

            MvxAndroidSetupSingleton.EnsureSingletonAvailable(this);
            MvxSingleton<MvxAndroidSetupSingleton>.Instance.EnsureInitialized();
            
            this.kernel.Load(
                new AndroidModelModule(),
                new ErrorReportingModule(pathToTemporaryFolder: basePath),
                new AndroidDataCollectionSharedKernelModule(basePath: basePath));

            CrashManager.Initialize(this);
            CrashManager.AttachSender(() => new FileReportSender("Interviewer", this.kernel.Get<IInfoFileSupplierRegistry>()));
         
            this.kernel.Bind<Context>().ToConstant(this);

            var liteEventBus = this.kernel.Get<ILiteEventBus>();
            this.kernel.Unbind<ILiteEventBus>();

            var cqrsEventBus = new InProcessEventBus(Kernel.Get<IEventStore>(), new EventBusSettings(),
                kernel.Get<ILogger>());

            var hybridEventBus = new HybridEventBus(liteEventBus, cqrsEventBus);

            kernel.Bind<IEventBus>().ToConstant(hybridEventBus);
            kernel.Bind<ILiteEventBus>().ToConstant(hybridEventBus);

            this.kernel.Bind<IInterviewerSettings>().To<InterviewerSettings>();
            this.kernel.Bind<ISynchronizationService>().To<SynchronizationService>();

            this.kernel.Bind<ISyncProtocolVersionProvider>().To<SyncProtocolVersionProvider>().InSingletonScope();
            
            this.InitFileStorage(cqrsEventBus);
            this.InitDashboard(cqrsEventBus);

            this.kernel.VerifyIfDebug();
        }

        private void RestoreAppState()
        {
            AndroidEnvironment.UnhandledExceptionRaiser += this.AndroidEnvironmentUnhandledExceptionRaiser;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                AndroidEnvironment.UnhandledExceptionRaiser -= this.AndroidEnvironmentUnhandledExceptionRaiser;
            }

            base.Dispose(disposing);
        }

        private void AndroidEnvironmentUnhandledExceptionRaiser(object sender, RaiseThrowableEventArgs e)
        {
            var splashActivity =  new Intent(this, typeof(SplashActivity));
            splashActivity.PutExtra("finish", true); // if you are checking for this in your other Activities
            splashActivity.AddFlags(ActivityFlags.ClearTask);
            splashActivity.AddFlags(ActivityFlags.ClearTop);
            splashActivity.AddFlags(ActivityFlags.NewTask);
            this.StartActivity(splashActivity);
        }

        private IKernel kernel;

        public override void OnLowMemory()
        {
            base.OnLowMemory();
            GC.Collect();
        }
    }
}