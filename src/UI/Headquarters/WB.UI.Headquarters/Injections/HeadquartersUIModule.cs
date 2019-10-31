using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using ASP;
using Microsoft.Owin.Security;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport;
using WB.Core.BoundedContexts.Headquarters.DataExport.DataExportDetails;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services;
using WB.Core.BoundedContexts.Headquarters.DataExport.Views;
using WB.Core.BoundedContexts.Headquarters.Implementation;
using WB.Core.GenericSubdomains.Portable.Implementation.Services;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.UI.Headquarters.Views;
using WB.UI.Shared.Web.Filters;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services;
using WB.Core.BoundedContexts.Headquarters.InterviewerProfiles;
using WB.Core.BoundedContexts.Headquarters.MoveUserToAnotherTeam;
using WB.Core.BoundedContexts.Headquarters.OwinSecurity;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Services;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.BoundedContexts.Headquarters.Views.InterviewHistory;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.Modularity;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Utils;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.Core.SharedKernels.SurveyManagement.Web.Code.CommandDeserialization;
using WB.Core.Synchronization.MetaInfo;
using WB.Enumerator.Native.WebInterview;
using WB.Enumerator.Native.WebInterview.Services;
using WB.Infrastructure.Native.Logging.Slack;
using WB.Infrastructure.Native.Questionnaire;
using WB.UI.Headquarters.API.WebInterview;
using WB.UI.Headquarters.API.WebInterview.Services;
using WB.UI.Headquarters.Code;
using WB.UI.Headquarters.Code.CommandTransformation;
using WB.UI.Headquarters.Filters;
using WB.UI.Headquarters.Services;
using WB.UI.Shared.Web.Attributes;
using WB.UI.Shared.Web.CommandDeserialization;
using WB.UI.Shared.Web.Configuration;
using WB.UI.Shared.Web.Implementation.Services;
using WB.UI.Shared.Web.Modules;
using WB.UI.Shared.Web.Services;
using WB.UI.Shared.Web.Slack;

namespace WB.UI.Headquarters.Injections
{
    public class HeadquartersUIModule : IWebModule
    {
        public void Load(IWebIocRegistry registry)
        {
            registry.BindAsSingleton<IInterviewAnswerSerializer, NewtonInterviewAnswerJsonSerializer>();
            registry.Bind<ISlackApiClient, SlackApiClient>();
            registry.BindAsSingleton<ISlackMessageThrottler, SlackMessageThrottler>();
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
            registry.Bind<IVirtualPathService, VirtualPathService>();

            registry.Bind<IVersionCheckService, VersionCheckService>();
            registry.BindAsSingleton<IHttpStatistician, HttpStatistician>();
            registry.BindAsSingleton<IAudioProcessingService, AudioProcessingService>();
            registry.Bind<IDataExportStatusReader, DataExportStatusReader>();

            registry.BindAsSingleton<IRestServiceSettings, DesignerQuestionnaireApiRestServiceSettings>();

            registry.RegisterDenormalizer<InterviewLifecycleEventHandler>();

            registry.Bind<IHttpClientFactory, DefaultHttpClientFactory>();

            registry.Bind<IFastBinaryFilesHttpHandler, FastBinaryFilesHttpHandler>();

            registry.BindMvcActionFilter<UnderConstructionMvcFilter>();
            registry.BindWebApiFilter<UnderConstructionHttpFilter>();
            registry.BindMvcActionFilter<GlobalNotificationAttribute>();

            registry.BindMvcActionFilterWhenControllerOrActionHasNoAttribute<TransactionFilter, NoTransactionAttribute>(1);
            registry.BindWebApiActionFilterWhenControllerOrActionHasNoAttribute<ApiTransactionFilter, NoTransactionAttribute>();

            registry.Bind<ICommandDeserializer, SurveyManagementCommandDeserializer>();
            registry.Bind<ICommandTransformator, CommandTransformator>();
            
            registry.Bind<IInterviewerVersionReader, InterviewerVersionReader>();
            registry.Bind<IWebInterviewAllowService, WebInterviewAllowService>();
            registry.Bind<IReviewAllowedService, ReviewAllowedService>();
            registry.Bind<IInterviewerProfileFactory, InterviewerProfileFactory>();
            registry.Bind<ITranslationsExportService, TranslationsExportService>();
            registry.Bind<IQuestionnaireExporter, QuestionnaireExporter>();

            registry.Bind<IClientApkProvider, ClientApkProvider>();
            registry.Bind<IQRCodeHelper, QRCodeHelper>();

            registry.BindAsSingleton<ILocalExportServiceRunner, LocalExportServiceRunner>();
            registry.BindAsSingleton<IApplicationPathResolver, AspNetAppPathResolver>();
            registry.Bind<IDesignerUserCredentials, DesignerUserCredentials>();

            registry.BindToMethodInRequestScope<IAuthenticationManager>(context => HttpContext.Current.GetOwinContext().Authentication);
            registry.Bind<HqSignInManager>();

            registry.BindToMethod<IExportServiceApi>(ctx =>
            {
                var settings = ctx.Get<InterviewDataExportSettings>();
                var cfg = ctx.Get<IConfigurationManager>();

                string key = null;

                var localRunner = ctx.Get<ILocalExportServiceRunner>();
                localRunner.Run();

                var exportServiceSettings = ctx.Get<IPlainKeyValueStorage<ExportServiceSettings>>();
                key = exportServiceSettings.GetById(AppSetting.ExportServiceStorageKey).Key;

                var http = new HttpClient
                {
                    BaseAddress = new Uri(settings.ExportServiceUrl),
                    DefaultRequestHeaders =
                    {
                        Authorization = new AuthenticationHeaderValue(@"Bearer", key),
                        Referrer = GetBaseUrl(cfg)
                    }
                };

                http.DefaultRequestHeaders.Add(@"x-tenant-name", cfg.AppSettings[@"Storage.S3.Prefix"]);

                var api = Refit.RestService.For<IExportServiceApi>(http);

                return api;
            });
        }

        public Task Init(IServiceLocator serviceLocator, UnderConstructionInfo status)
        {
            return Task.CompletedTask;
        }

        public Uri GetBaseUrl(IConfigurationManager cfg)
        {
            var uri = cfg.AppSettings[@"BaseUrl"];
            if (!string.IsNullOrWhiteSpace(uri))
            {
                return new Uri(uri);
            }

            if (HttpContext.Current != null)
            {
                var request = HttpContext.Current.Request;
                var appUrl = HttpRuntime.AppDomainAppVirtualPath;

                if (appUrl != "/")
                    appUrl = "/" + appUrl;

                var baseUrl = string.Format("{0}://{1}{2}", request.UrlScheme(), request.Url.Authority, appUrl);

                return new Uri(baseUrl);
            }

            return null;
        }
    }
}
