using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Android.App;
using Android.Content;
using Android.Runtime;
using Cirrious.MvvmCross.Droid.Platform;
using Main.Core.Events.Questionnaire;
using Main.DenormalizerStorage;
using Microsoft.Practices.ServiceLocation;
using Mono.Android.Crasher;
using Mono.Android.Crasher.Attributes;
using Mono.Android.Crasher.Data.Submit;
using Ncqrs;
using Ncqrs.Domain.Storage;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ncqrs.Eventing.Sourcing.Snapshotting;
using Ncqrs.Eventing.Storage;
using Ninject;
using Ninject.Activation;
using Ninject.Modules;
using WB.Core.BoundedContexts.Capi;
using WB.Core.BoundedContexts.Capi.EventHandler;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.Core.BoundedContexts.Supervisor.Factories;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.Files;
using WB.Core.Infrastructure.Ncqrs;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Commands.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement;
using WB.UI.QuestionnaireTester.Authentication;
using WB.UI.QuestionnaireTester.Services;
using WB.UI.Shared.Android;
using WB.UI.Shared.Android.Controls.ScreenItems;
using Context = Android.Content.Context;

namespace WB.UI.QuestionnaireTester
{
#if DEBUG 
    [Application(Debuggable=true)] 
#else
    [Application(Debuggable = false)]
#endif

    [Crasher(UseCustomData = false)]
    public class CapiTesterApplication : Application
    {
        public class ServiceLocationModule : NinjectModule
        {
            public override void Load()
            {
                ServiceLocator.SetLocatorProvider(() => new NinjectServiceLocator(this.Kernel));
                this.Kernel.Bind<IServiceLocator>().ToMethod(_ => ServiceLocator.Current);
            }
        }

        #region static properties

        public static TOutput LoadView<TInput, TOutput>(TInput input)
        {
            var factory = Kernel.TryGet<IViewFactory<TInput, TOutput>>();

            return factory == null ? default(TOutput) : factory.Load(input);
        }

        public static ICommandService CommandService
        {
            get { return Kernel.Get<ICommandService>(); }
        }

        public static DesignerAuthentication DesignerMembership
        {
            get { return Kernel.Get<DesignerAuthentication>(); }
        }

        public static DesignerService DesignerServices
        {
            get { return Kernel.Get<DesignerService>(); }
        }

        private const string DesignerPath = "DesignerPath";

        public static string GetPathToDesigner()
        {
            ISharedPreferences prefs = Context.GetSharedPreferences(Context.Resources.GetString(Resource.String.ApplicationName), FileCreationMode.Private);
            var pathToDesigner = prefs.GetString(DesignerPath, Context.Resources.GetString(Resource.String.DesignerPath));
            #if DEBUG
            //pathToDesigner = "http://172.29.124.72/Designer/api/tester";
            #endif
            return pathToDesigner;
        }

        public static void SetPathToDesigner(string path)
        {
            ISharedPreferences prefs = Application.Context.GetSharedPreferences(Context.Resources.GetString(Resource.String.ApplicationName),
             FileCreationMode.Private);
            ISharedPreferencesEditor prefEditor = prefs.Edit();
            prefEditor.PutString(DesignerPath, path);
            prefEditor.Commit();
        }

        public static IKernel Kernel
        {
            get
            {
                if (Context == null)
                    return null;
                var capiApp = Context.ApplicationContext as CapiTesterApplication;
                if (capiApp == null)
                    return null;
                return capiApp.kernel;
            }
        }

        #endregion


        protected CapiTesterApplication(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
        }

        private void InitInterviewStorage(InProcessEventBus bus)
        {
            var eventHandler =
                new InterviewViewModelDenormalizer(
                    this.kernel.Get<IReadSideRepositoryWriter<InterviewViewModel>>(), this.kernel.Get<IReadSideKeyValueStorage<QuestionnaireDocumentVersioned>>(),
                    this.kernel.Get<IReadSideKeyValueStorage<QuestionnaireRosterStructure>>(), this.kernel.Get<IQuestionnaireRosterStructureFactory>());

            bus.RegisterHandler(eventHandler, typeof (InterviewSynchronized));
            bus.RegisterHandler(eventHandler, typeof (MultipleOptionsQuestionAnswered));
            bus.RegisterHandler(eventHandler, typeof (NumericIntegerQuestionAnswered));
            bus.RegisterHandler(eventHandler, typeof (NumericRealQuestionAnswered));
            bus.RegisterHandler(eventHandler, typeof (TextQuestionAnswered));
            bus.RegisterHandler(eventHandler, typeof (TextListQuestionAnswered));
            bus.RegisterHandler(eventHandler, typeof (SingleOptionQuestionAnswered));
            bus.RegisterHandler(eventHandler, typeof (DateTimeQuestionAnswered));
            bus.RegisterHandler(eventHandler, typeof (GroupsDisabled));
            bus.RegisterHandler(eventHandler, typeof (GroupsEnabled));
            bus.RegisterHandler(eventHandler, typeof (QuestionsDisabled));
            bus.RegisterHandler(eventHandler, typeof (QuestionsEnabled));
            bus.RegisterHandler(eventHandler, typeof (AnswersDeclaredInvalid));
            bus.RegisterHandler(eventHandler, typeof (AnswersDeclaredValid));
            bus.RegisterHandler(eventHandler, typeof(AnswerCommented));
            bus.RegisterHandler(eventHandler, typeof(InterviewCompleted));
            bus.RegisterHandler(eventHandler, typeof(InterviewRestarted));
            bus.RegisterHandler(eventHandler, typeof(GroupPropagated));
            bus.RegisterHandler(eventHandler, typeof(RosterInstancesAdded));
            bus.RegisterHandler(eventHandler, typeof(RosterInstancesRemoved));
            bus.RegisterHandler(eventHandler, typeof(SynchronizationMetadataApplied));
            bus.RegisterHandler(eventHandler, typeof(GeoLocationQuestionAnswered));
            bus.RegisterHandler(eventHandler, typeof(AnswersRemoved));
            bus.RegisterHandler(eventHandler, typeof(SingleOptionLinkedQuestionAnswered));
            bus.RegisterHandler(eventHandler, typeof(MultipleOptionsLinkedQuestionAnswered));
            
            bus.RegisterHandler(eventHandler, typeof(RosterInstancesTitleChanged));
            bus.RegisterHandler(eventHandler, typeof(QRBarcodeQuestionAnswered));

            bus.RegisterHandler(eventHandler, typeof(PictureQuestionAnswered));
            bus.RegisterHandler(eventHandler, typeof(TextListQuestionAnswered));

            bus.RegisterHandler(eventHandler, typeof(InterviewForTestingCreated));

            var answerOptionsForLinkedQuestionsDenormalizer = this.kernel.Get<AnswerOptionsForLinkedQuestionsDenormalizer>();

            bus.RegisterHandler(answerOptionsForLinkedQuestionsDenormalizer, typeof(AnswersRemoved));
            bus.RegisterHandler(answerOptionsForLinkedQuestionsDenormalizer, typeof(TextQuestionAnswered));
            bus.RegisterHandler(answerOptionsForLinkedQuestionsDenormalizer, typeof(NumericIntegerQuestionAnswered));
            bus.RegisterHandler(answerOptionsForLinkedQuestionsDenormalizer, typeof(NumericRealQuestionAnswered));
            bus.RegisterHandler(answerOptionsForLinkedQuestionsDenormalizer, typeof(DateTimeQuestionAnswered));

            var answerOptionsForCascadingQuestionsDenormalizer = this.kernel.Get<AnswerOptionsForCascadingQuestionsDenormalizer>();
           
            bus.RegisterHandler(answerOptionsForCascadingQuestionsDenormalizer, typeof(AnswersRemoved));
            bus.RegisterHandler(answerOptionsForCascadingQuestionsDenormalizer, typeof(SingleOptionQuestionAnswered));
        }

        private void InitTemplateStorage(InProcessEventBus bus)
        {
            var templateDenoramalizer = new QuestionnaireDenormalizer(
                this.kernel.Get<IReadSideKeyValueStorage<QuestionnaireDocumentVersioned>>(),
                this.kernel.Get<IPlainQuestionnaireRepository>());

            bus.RegisterHandler(templateDenoramalizer, typeof(TemplateImported));
            bus.RegisterHandler(templateDenoramalizer, typeof(QuestionnaireDeleted));
            bus.RegisterHandler(templateDenoramalizer, typeof(PlainQuestionnaireRegistered));
            
            var propagationStructureDenormalizer = new QuestionnaireRosterStructureDenormalizer(
                this.kernel.Get<IReadSideKeyValueStorage<QuestionnaireRosterStructure>>(),
                this.kernel.Get<IQuestionnaireRosterStructureFactory>(),
                this.kernel.Get<IPlainQuestionnaireRepository>());

            bus.RegisterHandler(propagationStructureDenormalizer, typeof(TemplateImported));
            bus.RegisterHandler(propagationStructureDenormalizer, typeof(QuestionnaireDeleted));
            bus.RegisterHandler(propagationStructureDenormalizer, typeof(PlainQuestionnaireRegistered));
        }

        public override void OnCreate()
        {
            base.OnCreate();

            CrashManager.Initialize(this);
            CrashManager.AttachSender(() => new FileReportSender("Capi.Tester"));
            //this.RestoreAppState();

            // initialize app if necessary
            MvxAndroidSetupSingleton.EnsureSingletonAvailable(this);
            MvxAndroidSetupSingleton.Instance.EnsureInitialized();

            var basePath = Directory.Exists(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal))
                   ? System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal)
                   : Android.OS.Environment.ExternalStorageDirectory.AbsolutePath;

            this.kernel = new StandardKernel(
                new ServiceLocationModule(),
                new InfrastructureModule().AsNinject(),
                new NcqrsModule().AsNinject(),
                new CapiTesterCoreRegistry(),
                new CapiBoundedContextModule(),
                new AndroidSharedModule(),
                new TesterLoggingModule(),
                new AndroidTesterModelModule(),
                new DataCollectionSharedKernelModule(usePlainQuestionnaireRepository: false, basePath: basePath),
                new FileInfrastructureModule());

            this.kernel.Bind<IAuthentication, DesignerAuthentication>().ToConstant(new DesignerAuthentication());
            this.kernel.Bind<DesignerService>().ToSelf();
            
            this.kernel.Bind<Context>().ToConstant(this);

            NcqrsEnvironment.SetDefault(ServiceLocator.Current.GetInstance<ILogger>());
            NcqrsEnvironment.InitDefaults();

            kernel.Unbind<IQuestionnaireAssemblyFileAccessor>();
            kernel.Bind<IQuestionnaireAssemblyFileAccessor>().To<QuestionnareAssemblyTesterFileAccessor>().InSingletonScope();

            NcqrsEnvironment.SetDefault<ISnapshottingPolicy>(new SimpleSnapshottingPolicy(1));

            kernel.Bind<ISnapshottingPolicy>().ToMethod(context => NcqrsEnvironment.Get<ISnapshottingPolicy>());
            kernel.Bind<IAggregateRootCreationStrategy>().ToMethod(context => NcqrsEnvironment.Get<IAggregateRootCreationStrategy>());
            kernel.Bind<IAggregateSnapshotter>().ToMethod(context => NcqrsEnvironment.Get<IAggregateSnapshotter>());

            var bus = new InProcessEventBus(Kernel.Get<IEventStore>());
            NcqrsEnvironment.SetDefault<IEventBus>(bus);
            //kernel.Bind<IEventBus>().ToConstant(bus);
            this.kernel.Bind<IEventBus>().ToConstant(bus).Named("interviewViewBus");

            NcqrsEnvironment.SetDefault<IEventStore>(Kernel.Get<IEventStore>());

            this.kernel.Unbind<IAnswerOnQuestionCommandService>();
            this.kernel.Bind<IAnswerOnQuestionCommandService>().To<AnswerOnQuestionCommandService>().InSingletonScope();
            this.kernel.Bind<IAnswerProgressIndicator>().To<AnswerProgressIndicator>().InSingletonScope();
            this.kernel.Bind<IQuestionViewFactory>().To<DefaultQuestionViewFactory>();
            
            #region register handlers

            this.InitInterviewStorage(bus);
            this.InitTemplateStorage(bus);

            #endregion
        }

        private IKernel kernel;

        public override void OnLowMemory()
        {
            base.OnLowMemory();
            GC.Collect();
        }
    }

    public abstract class CoreRegistry : NinjectModule
    {
        protected virtual IEnumerable<Assembly> GetAssembliesForRegistration()
        {
            return new[] { (typeof(CoreRegistry)).Assembly };
        }

        /// <summary>
        /// Gets pairs of interface/type which should be registered.
        /// Usually is used to return implementation of interfaces declared not in assemblies returned by GetAssemblies method.
        /// </summary>
        /// <returns>Pairs of interface/implementation.</returns>
        protected virtual IEnumerable<KeyValuePair<Type, Type>> GetTypesForRegistration()
        {
            return Enumerable.Empty<KeyValuePair<Type, Type>>();
        }

        public override void Load()
        {
            RegisterDenormalizers();
            RegisterEventHandlers();
            RegisterAdditionalElements();
        }

        protected virtual void RegisterAdditionalElements()
        {
            foreach (KeyValuePair<Type, Type> customBindType in this.GetTypesForRegistration())
            {
                this.Kernel.Bind(customBindType.Key).To(customBindType.Value);
            }
        }

        protected virtual void RegisterViewFactories()
        {
            BindInterface(this.GetAssembliesForRegistration(), typeof(IViewFactory<,>), (c) => Guid.NewGuid());
        }

        protected virtual void RegisterEventHandlers()
        {
            BindInterface(this.GetAssembliesForRegistration(), typeof(IEventHandler<>), (c) => this.Kernel);
        }

        protected virtual void RegisterDenormalizers()
        {
            // currently in-memory repo accessor also contains repository itself as internal dictionary, so we need to create him as singletone
            this.Kernel.Bind(typeof(InMemoryReadSideRepositoryAccessor<>)).ToSelf().InSingletonScope();

            this.Kernel.Bind(typeof(IReadSideRepositoryReader<>)).ToMethod(this.GetInMemoryReadSideRepositoryAccessor);
            this.Kernel.Bind(typeof(IQueryableReadSideRepositoryReader<>)).ToMethod(this.GetInMemoryReadSideRepositoryAccessor);
            this.Kernel.Bind(typeof(IReadSideRepositoryWriter<>)).ToMethod(this.GetInMemoryReadSideRepositoryAccessor);
        }

        protected object GetInMemoryReadSideRepositoryAccessor(IContext context)
        {
            var genericParameter = context.GenericArguments[0];

            return this.Kernel.Get(typeof(InMemoryReadSideRepositoryAccessor<>).MakeGenericType(genericParameter));
        }

        protected void BindInterface(IEnumerable<Assembly> assembyes, Type interfaceType, Func<IContext, object> scope)
        {

            var implementations =
             assembyes.SelectMany(a => a.GetTypes()).Where(t => t.IsPublic && ImplementsAtLeastOneInterface(t, interfaceType));
            foreach (Type implementation in implementations)
            {
                if (interfaceType != typeof(IViewFactory<,>))
                {
                    this.Kernel.Bind(interfaceType).To(implementation).InScope(scope);
                }
                if (interfaceType.IsGenericType)
                {
                    var interfaceImplementations =
                        implementation.GetInterfaces().Where(i => IsInterfaceInterface(i, interfaceType));
                    foreach (Type interfaceImplementation in interfaceImplementations)
                    {
                        this.Kernel.Bind(interfaceType.MakeGenericType(interfaceImplementation.GetGenericArguments())).
                            To(implementation).InScope(scope);
                    }
                }
            }
        }

        private bool ImplementsAtLeastOneInterface(Type type, Type interfaceType)
        {
            return type.IsClass && !type.IsAbstract &&
                   type.GetInterfaces().Any(i => IsInterfaceInterface(i, interfaceType));
        }

        private bool IsInterfaceInterface(Type type, Type interfaceType)
        {
            return type.IsInterface
                && ((interfaceType.IsGenericType && type.IsGenericType && type.GetGenericTypeDefinition() == interfaceType)
                    || (!type.IsGenericType && !interfaceType.IsGenericType && type == interfaceType));
        }
    }

    public class CapiTesterCoreRegistry : CoreRegistry
    {
        protected override IEnumerable<Assembly> GetAssembliesForRegistration()
        {
            return
                Enumerable.Concat(base.GetAssembliesForRegistration(), new[] { typeof(ImportFromDesignerForTester).Assembly, this.GetType().Assembly });
        }
    }
}