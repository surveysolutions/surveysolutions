using System.Threading.Tasks;
using WB.Core.BoundedContexts.Designer.Classifications;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Search;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.DependencyInjection;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.Modularity;
using WB.Infrastructure.Native.Files.Implementation.FileSystem;
using WB.Infrastructure.Native.Storage;
using WB.UI.Designer.Api.WebTester;
using WB.UI.Designer.Controllers.Api.WebTester;
using WB.UI.Designer.Implementation.Services;
using WB.UI.Designer.Services;

namespace WB.UI.Designer.Modules
{
    /// <summary>
    /// The main module.
    /// </summary>
    public class DesignerWebModule : IAppModule
    {
        public void Load(IDependencyRegistry registry)
        {
            //registry.Bind<ILog>().ToConstant(new Log()).InSingletonScope();
            //registry.BindMvcExceptionFilter<CustomHandleErrorFilter>();
            //registry.BindMvcAuthorizationFilter<CustomAuthorizeFilter>();

            registry.Bind<IJsonAllTypesSerializer, JsonAllTypesSerializer>();

            registry.Bind<IQuestionnairePackageComposer, QuestionnairePackageComposer>();
            registry.BindAsSingleton<IQuestionnaireCacheStorage, QuestionnaireCacheStorage>();
            registry.Bind<IArchiveUtils, ZipArchiveUtils>();

            //registry.Bind<IRecipientNotifier, MailNotifier>();
            registry.Bind<IQuestionnaireSearchStorage, QuestionnaireSearchStorage>();
            registry.Bind<IClassificationsStorage, ClassificationsStorage>();

//            registry.BindToMethod<WebTesterSettings>(() =>
//            {
//                var appSettings = System.Configuration.ConfigurationManager.AppSettings;
//                return new WebTesterSettings
//                {
//                    ExpirationAmountMinutes = appSettings["WebTester.ExpirationAmountMinutes"].AsInt(),
//                    BaseUri = appSettings["WebTester.BaseUri"]
//                };
//            });

            registry.BindAsSingleton<IWebTesterService, WebTesterService>();
        }

        public Task InitAsync(IServiceLocator serviceLocator, UnderConstructionInfo status)
        {
            return Task.CompletedTask;
        }
    }
}
