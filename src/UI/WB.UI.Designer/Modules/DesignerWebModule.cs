using System.Threading.Tasks;
using AutoMapper;
using Serilog.Extensions.Logging;
using WB.Core.BoundedContexts.Designer.Classifications;
using WB.Core.BoundedContexts.Designer.ImportExport;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Search;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.DependencyInjection;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.Modularity;
using WB.Infrastructure.Native.Files.Implementation.FileSystem;
using WB.Infrastructure.Native.Storage;
using WB.UI.Designer.Api.WebTester;
using WB.UI.Designer.Code;
using WB.UI.Designer.Code.ImportExport;
using WB.UI.Designer.Controllers.Api.WebTester;
using WB.UI.Designer.Services;
using ILoggerProvider = Microsoft.Extensions.Logging.ILoggerProvider;

namespace WB.UI.Designer.Modules
{
    /// <summary>
    /// The main module.
    /// </summary>
    public class DesignerWebModule : IAppModule
    {
        public void Load(IDependencyRegistry registry)
        {
            registry.BindAsScoped<LocalOrDevelopmentAccessOnlyAttribute, LocalOrDevelopmentAccessOnlyAttribute>();

            registry.Bind<IJsonAllTypesSerializer, JsonAllTypesSerializer>();

            registry.Bind<IQuestionnairePackageComposer, QuestionnairePackageComposer>();
            registry.BindAsSingleton<IQuestionnaireCacheStorage, QuestionnaireCacheStorage>();
            registry.Bind<IArchiveUtils, ZipArchiveUtils>();

            registry.Bind<IQuestionnaireSearchStorage, QuestionnaireSearchStorage>();
            registry.Bind<IClassificationsStorage, ClassificationsStorage>();
            registry.BindAsSingleton<IWebTesterService, WebTesterService>();          
            registry.BindAsSingleton<ILoggerProvider, SerilogLoggerProvider>();   
            
            registry.Bind<IImportExportQuestionnaireMapper, ImportExportQuestionnaireMapper>();   
            registry.Bind<IQuestionnaireSerializer, QuestionnaireSerializer>();   
            registry.Bind<ITranslationImportExportService, TranslationImportExportService>();   
            
            registry.BindToConstant<IMapper>(_ => new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new QuestionnaireAutoMapperProfile());
                cfg.ConstructServicesUsing(_.GetService);
            }).CreateMapper());
        }

        public Task InitAsync(IServiceLocator serviceLocator, UnderConstructionInfo status)
        {
            return Task.CompletedTask;
        }
    }
}
