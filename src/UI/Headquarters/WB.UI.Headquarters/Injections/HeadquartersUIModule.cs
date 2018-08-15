using System.Threading.Tasks;
using System.Web.Mvc;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport;
using WB.Core.BoundedContexts.Headquarters.DataExport.DataExportDetails;
using WB.Core.BoundedContexts.Headquarters.DataExport.Security;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.Implementation.Services;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.SurveyManagement.Web.Code;
using WB.UI.Headquarters.Views;
using WB.UI.Shared.Web.Filters;
using WB.Core.BoundedContexts.Headquarters.Implementation;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services;
using WB.Core.BoundedContexts.Headquarters.IntreviewerProfiles;
using WB.Core.BoundedContexts.Headquarters.MoveUserToAnotherTeam;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Services;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.Modularity;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.Core.SharedKernels.SurveyManagement.Web.Code.CommandDeserialization;
using WB.Core.Synchronization.MetaInfo;
using WB.Enumerator.Native.WebInterview.Services;
using WB.Infrastructure.Native.Files.Implementation.FileSystem;
using WB.Infrastructure.Native.Questionnaire;
using WB.Infrastructure.Native.Storage;
using WB.UI.Headquarters.API.WebInterview;
using WB.UI.Headquarters.API.WebInterview.Services;
using WB.UI.Headquarters.Code;
using WB.UI.Headquarters.Filters;
using WB.UI.Headquarters.Services;
using WB.UI.Shared.Web.Attributes;
using WB.UI.Shared.Web.CommandDeserialization;
using WB.UI.Shared.Web.Modules;
using WB.UI.Shared.Web.Services;

namespace WB.UI.Headquarters.Injections
{
    public class HeadquartersUIModule : IWebModule
    {
        public void Load(IWebIocRegistry registry)
        {
            registry.Bind<IAssignmentsImportReader, AssignmentsImportReader>();
            registry.Bind<IAssignmentsImportFileConverter, AssignmentsImportFileConverter>();
            registry.Bind<IAssignmentsImportService, AssignmentsImportService>();
            registry.Bind<IFormDataConverterLogger, FormDataConverterLogger>();
            registry.Bind<IInterviewTreeBuilder, InterviewTreeBuilder>();
            registry.Bind<IInterviewExpressionStateUpgrader, InterviewExpressionStateUpgrader>();
            registry.Bind<IMetaInfoBuilder, MetaInfoBuilder>();
            registry.Bind<IUserImportService, UserImportService>();
            registry.Bind<IMoveUserToAnotherTeamService, MoveUserToAnotherTeamService>();
            registry.Bind<ISupportedVersionProvider, SupportedVersionProvider>();
            registry.Bind<IDataExportProcessDetails, DataExportProcessDetails>();

            registry.Bind<IExceptionFilter, HandleUIExceptionAttribute>();

            registry.Bind<IImageProcessingService, ImageProcessingService>();

            registry.BindAsSingleton<IVersionCheckService, VersionCheckService>();
            registry.BindAsSingleton<IHttpStatistician, HttpStatistician>();
            registry.BindAsSingleton<IAudioProcessingService, AudioProcessingService>();

            registry.BindAsSingleton<IRestServiceSettings, DesignerQuestionnaireApiRestServiceSettings>();

            registry.Bind<IRestService, RestService>(
                new ConstructorArgument("networkService", _ => null),
                new ConstructorArgument("restServicePointManager", _ => null),
                new ConstructorArgument("httpStatistican", _ => _.Resolve<IHttpStatistician>()));

            registry.BindHttpFilter<UnderConstructionHttpFilter>(System.Web.Http.Filters.FilterScope.Global, 0);
            registry.BindMvcFilter<UnderConstructionMvcFilter>(FilterScope.First, 0);
            registry.BindMvcFilterWhenActionMethodHasNoAttribute<TransactionFilter, NoTransactionAttribute>(FilterScope.First, 0);
            registry.BindMvcFilterWhenActionMethodHasNoAttribute<PlainTransactionFilter, NoTransactionAttribute>(FilterScope.First, 0);
            registry.BindHttpFilterWhenActionMethodHasNoAttribute<ApiTransactionFilter, NoTransactionAttribute>(System.Web.Http.Filters.FilterScope.Controller);
            registry.BindHttpFilterWhenActionMethodHasNoAttribute<PlainApiTransactionFilter, NoTransactionAttribute>(System.Web.Http.Filters.FilterScope.Controller);
            registry.BindMvcFilterWhenActionMethodHasNoAttribute<GlobalNotificationAttribute, NoTransactionAttribute>(FilterScope.Global, null);

            //this.Bind<IUserWebViewFactory>().To<UserWebViewFactory>(); // binded automatically but should not
            registry.Bind<ICommandDeserializer, SurveyManagementCommandDeserializer>();
            registry.BindAsSingleton<IRevalidateInterviewsAdministrationService, RevalidateInterviewsAdministrationService>();
            registry.BindAsSingleton<IInterviewerVersionReader, InterviewerVersionReader>();
            registry.Bind<IWebInterviewAllowService, WebInterviewAllowService>();
            registry.Bind<IReviewAllowedService, ReviewAllowedService>();
            registry.Bind<IInterviewerProfileFactory, InterviewerProfileFactory>();
            registry.Bind<ITranslationsExportService, TranslationsExportService>();
            registry.Bind<IQuestionnaireExporter, QuestionnaireExporter>();

            registry.Bind<IQRCodeHelper, QRCodeHelper>();
        }

        public Task Init(IServiceLocator serviceLocator, UnderConstructionInfo status)
        {
            return Task.CompletedTask;
        }
    }
}
