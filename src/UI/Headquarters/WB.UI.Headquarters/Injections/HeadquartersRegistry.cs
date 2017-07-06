using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using Main.DenormalizerStorage;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ninject;
using Ninject.Activation;
using Ninject.Modules;
using Ninject.Web.Mvc.FilterBindingSyntax;
using Ninject.Web.WebApi.FilterBindingSyntax;
using WB.Core.BoundedContexts.Headquarters;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport;
using WB.Core.BoundedContexts.Headquarters.DataExport.DataExportDetails;
using WB.Core.BoundedContexts.Headquarters.DataExport.Security;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.Implementation.Services;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Web.Code;
using WB.UI.Headquarters.Views;
using WB.UI.Shared.Web.Filters;
using WB.Core.BoundedContexts.Headquarters.Implementation;
using WB.Core.BoundedContexts.Headquarters.Implementation.SampleRecordsAccessors;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Services;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.MaskFormatter;
using WB.Core.SharedKernels.SurveyManagement.Web.Code.CommandDeserialization;
using WB.Core.Synchronization.MetaInfo;
using WB.Infrastructure.Native.Files.Implementation.FileSystem;
using WB.Infrastructure.Native.Storage;
using WB.UI.Headquarters.Code;
using WB.UI.Headquarters.Filters;
using WB.UI.Headquarters.Services;
using WB.UI.Shared.Web.Attributes;
using WB.UI.Shared.Web.CommandDeserialization;

namespace WB.UI.Headquarters.Injections
{
    public abstract class CoreRegistry : NinjectModule
    {
        protected virtual IEnumerable<Assembly> GetAssembliesForRegistration()
        {
            return new[] { (typeof(CoreRegistry)).Assembly };
        }

        public override void Load()
        {
            RegisterDenormalizers();
            RegisterEventHandlers();

            this.Kernel.Bind<IInterviewImportService>().To<InterviewImportService>();
            this.Kernel.Bind<IFormDataConverterLogger>().To<FormDataConverterLogger>();
            this.Kernel.Bind<IMaskedFormatter>().To<MaskedFormatter>();
            this.Kernel.Bind<IInterviewTreeBuilder>().To<InterviewTreeBuilder>();
            this.Kernel.Bind<IInterviewExpressionStateUpgrader>().To<InterviewExpressionStateUpgrader>();
            this.Kernel.Bind<IMetaInfoBuilder>().To<MetaInfoBuilder>();
            this.Kernel.Bind<IInterviewEntityViewFactory>().To<InterviewEntityViewFactory>();
            this.Kernel.Bind<IInterviewDataAndQuestionnaireMerger>().To<InterviewDataAndQuestionnaireMerger>();
            this.Kernel.Bind<IUserPreloadingService>().To<UserPreloadingService>();
            this.Kernel.Bind<IAttachmentContentService>().To<AttachmentContentService>();
            this.Kernel.Bind<ISupportedVersionProvider>().To<SupportedVersionProvider>();
            this.Kernel.Bind<IDataExportProcessDetails>().To<DataExportProcessDetails>();
            
            this.Kernel.Bind<IRecordsAccessor>().To<CsvRecordsAccessor>();
            this.Kernel.Bind<IExceptionFilter>().To<HandleUIExceptionAttribute>();

            this.Kernel.Bind<IAssemblyService>().To<AssemblyService>();
            this.Kernel.Bind<IImageProcessingService>().To<ImageProcessingService>();

            this.Kernel.Bind<IVersionCheckService>().To<VersionCheckService>().InSingletonScope();
            this.Kernel.Bind<IHttpStatistician>().To<HttpStatistician>().InSingletonScope();
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
                this.Kernel.Bind(interfaceType).To(implementation).InScope(scope);
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

    public class HeadquartersRegistry : CoreRegistry
    {
        protected override IEnumerable<Assembly> GetAssembliesForRegistration()
        {
            return base.GetAssembliesForRegistration().Concat(new[]
            {
                typeof(HeadquartersRegistry).Assembly,
                typeof(QuestionnaireItemInputModel).Assembly,
                typeof(HeadquartersBoundedContextModule).Assembly
            });
        }

        protected override void RegisterDenormalizers() { }
        
        protected override void RegisterEventHandlers()
        {
            base.RegisterEventHandlers();

            this.BindInterface(this.GetAssembliesForRegistration(), typeof(IEventHandler), (c) => this.Kernel);
        }

        public override void Load()
        {
            base.Load();

            this.Bind<ISerializer>().ToMethod((ctx) => new NewtonJsonSerializer());
            this.Bind<IInterviewAnswerSerializer>().ToMethod(ctx => new NewtonInterviewAnswerJsonSerializer());
            
            this.Bind<IJsonAllTypesSerializer>().ToMethod(ctx => new JsonAllTypesSerializer());

            this.Bind<IStringCompressor>().To<JsonCompressor>();
            this.Bind<IRestServiceSettings>().To<DesignerQuestionnaireApiRestServiceSettings>().InSingletonScope();

            this.Bind<IRestService>()
                .To<RestService>()
                .WithConstructorArgument("networkService", _ => null)
                .WithConstructorArgument("restServicePointManager", _ => null)
                .WithConstructorArgument("httpStatistican", _ => _.Kernel.Get<IHttpStatistician>());

            this.Bind<IExportSettings>().To<ExportSettings>();

            this.Bind<IArchiveUtils, IProtectedArchiveUtils>().To<ZipArchiveUtils>();

            this.BindFilter<TransactionFilter>(FilterScope.First, 0)
                .WhenActionMethodHasNo<NoTransactionAttribute>();
            this.BindFilter<PlainTransactionFilter>(FilterScope.First, 0)
                .WhenActionMethodHasNo<NoTransactionAttribute>();

            this.BindHttpFilter<ApiTransactionFilter>(System.Web.Http.Filters.FilterScope.Controller)
                .When((controllerContext, actionDescriptor) => !actionDescriptor.GetCustomAttributes(typeof(NoTransactionAttribute)).Any());
            this.BindHttpFilter<PlainApiTransactionFilter>(System.Web.Http.Filters.FilterScope.Controller)
                .When((controllerContext, actionDescriptor) => !actionDescriptor.GetCustomAttributes(typeof(NoTransactionAttribute)).Any());
            this.BindFilter<GlobalNotificationAttribute>(FilterScope.Global, null)
                .WhenActionMethodHasNo<NoTransactionAttribute>();

            //this.Bind<IUserWebViewFactory>().To<UserWebViewFactory>(); // binded automatically but should not
            this.Bind<ICommandDeserializer>().To<SurveyManagementCommandDeserializer>();
            this.Bind<IRevalidateInterviewsAdministrationService>().To<RevalidateInterviewsAdministrationService>().InSingletonScope();
            this.Bind<IInterviewerVersionReader>().To<InterviewerVersionReader>().InSingletonScope();
        }
    }
}