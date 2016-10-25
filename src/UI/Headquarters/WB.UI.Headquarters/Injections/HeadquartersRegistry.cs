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
using WB.Core.BoundedContexts.Headquarters.DataExport.DataExportDetails;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.Implementation.Services;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Web.Code;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Security;
using WB.UI.Headquarters.Views;
using WB.UI.Shared.Web.Filters;
using WB.Infrastructure.Security;
using WB.Core.BoundedContexts.Headquarters.Implementation;
using WB.Core.BoundedContexts.Headquarters.Implementation.SampleRecordsAccessors;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Services;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.MaskFormatter;
using WB.Core.SharedKernels.SurveyManagement.Web.Code.CommandDeserialization;
using WB.Core.SharedKernels.SurveyManagement.Web.Models.User;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;
using WB.Core.Synchronization.Implementation.ImportManager;
using WB.Core.Synchronization.MetaInfo;
using WB.Infrastructure.Native.Files.Implementation.FileSystem;
using WB.Infrastructure.Native.Storage;
using WB.UI.Headquarters.Code;
using WB.UI.Headquarters.Filters;
using WB.UI.Headquarters.Implementation.Services;
using WB.UI.Headquarters.Models.User;
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
            this.Kernel.Bind<IIdentityManager>().To<IdentityManager>();
            this.Kernel.Bind<IFormsAuthentication>().To<FormsAuthentication>();
            this.Kernel.Bind<IGlobalInfoProvider>().To<GlobalInfoProvider>();
            this.Kernel.Bind<IMaskedFormatter>().To<MaskedFormatter>();
            this.Kernel.Bind<IInterviewExpressionStateUpgrader>().To<InterviewExpressionStateUpgrader>();
            this.Kernel.Bind<IMetaInfoBuilder>().To<MetaInfoBuilder>();
            this.Kernel.Bind<IInterviewEntityViewFactory>().To<InterviewEntityViewFactory>();
            this.Kernel.Bind<IInterviewDataAndQuestionnaireMerger>().To<InterviewDataAndQuestionnaireMerger>();
            this.Kernel.Bind<IUserPreloadingService>().To<UserPreloadingService>();
            this.Kernel.Bind<IAttachmentContentService>().To<AttachmentContentService>();
            this.Kernel.Bind<ISupportedVersionProvider>().To<SupportedVersionProvider>();
            this.Kernel.Bind<IDataExportProcessDetails>().To<DataExportProcessDetails>();
            this.Kernel.Bind<IUserBrowseViewFactory>().To<UserBrowseViewFactory>();
            this.Kernel.Bind<IUserWebViewFactory>().To<UserWebViewFactory>();
            
            this.Kernel.Bind<IReadOnlyInterviewStateDependentOnAnswers>().To<InterviewStateDependentOnAnswers>();
            this.Kernel.Bind<IBackupManager>().To<DefaultBackupManager>();
            this.Kernel.Bind<IRecordsAccessor>().To<CsvRecordsAccessor>();

            this.Kernel.Bind<IExceptionFilter>().To<HandleUIExceptionAttribute>();
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
                typeof(QuestionnaireMembershipProvider).Assembly,
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

            this.Bind<IProtobufSerializer>().To<ProtobufSerializer>();

            this.Bind<ISerializer>().ToMethod((ctx) => new NewtonJsonSerializer());
            this.Bind<IJsonAllTypesSerializer>().ToMethod((ctx) => new JsonAllTypesSerializer());

            this.Bind<IStringCompressor>().To<JsonCompressor>();
            this.Bind<IRestServiceSettings>().To<DesignerQuestionnaireApiRestServiceSettings>().InSingletonScope();

            this.Bind<IRestService>().To<RestService>().WithConstructorArgument("networkService", _ => null).WithConstructorArgument("restServicePointManager", _=> null);

            this.Bind<IExportSettings>().To<ExportSettings>();

            this.Bind<IArchiveUtils, IZipArchiveProtectionService>().To<ZipArchiveUtils>();

            this.BindFilter<TransactionFilter>(FilterScope.First, 0)
                .WhenActionMethodHasNo<NoTransactionAttribute>();

            this.BindHttpFilter<ApiTransactionFilter>(System.Web.Http.Filters.FilterScope.Controller)
                .When((controllerContext, actionDescriptor) => !actionDescriptor.GetCustomAttributes(typeof(NoTransactionAttribute)).Any());

            this.BindFilter<PlainTransactionFilter>(FilterScope.First, 0)
                .WhenActionMethodHasNo<NoTransactionAttribute>();
            this.BindHttpFilter<PlainApiTransactionFilter>(System.Web.Http.Filters.FilterScope.Controller)
                .When((controllerContext, actionDescriptor) => !actionDescriptor.GetCustomAttributes(typeof(NoTransactionAttribute)).Any());
            this.BindFilter<GlobalNotificationAttribute>(FilterScope.Global, null)
                .WhenActionMethodHasNo<NoTransactionAttribute>();

            //this.Bind<IUserWebViewFactory>().To<UserWebViewFactory>(); // binded automatically but should not
            this.Bind<ICommandDeserializer>().To<SurveyManagementCommandDeserializer>();
            this.Bind<IRevalidateInterviewsAdministrationService>().To<RevalidateInterviewsAdministrationService>().InSingletonScope();
        }
    }
}