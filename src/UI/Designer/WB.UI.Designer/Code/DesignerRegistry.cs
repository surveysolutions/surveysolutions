using System.Threading.Tasks;
using System.Web.Mvc;
using WB.Core.BoundedContexts.Designer.Comments;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Services.CodeGeneration;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Pdf;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.Modularity;
using WB.UI.Designer.Code.ConfigurationManager;
using WB.UI.Designer.Code.Implementation;
using WB.UI.Designer.Implementation.Services;
using WB.UI.Designer.Mailers;
using WB.UI.Designer.Services;
using WB.UI.Shared.Web.Attributes;
using WB.UI.Shared.Web.Filters;
using WB.UI.Shared.Web.Modules;
using WB.UI.Shared.Web.Modules.Filters;

namespace WB.UI.Designer.Code
{
    public class DesignerRegistry : IWebModule
    {
        private readonly PdfSettings pdfSettings;
        private readonly DeskSettings deskSettings;
        private readonly QuestionnaireHistorySettings historySettings;

        public DesignerRegistry(PdfConfigSection pdfConfigSettings, DeskConfigSection deskSettings, int questionnaireChangeHistoryLimit)
        {
            this.pdfSettings = new PdfSettings(
                pdfConfigSettings.InstructionsExcerptLength.Value,
                pdfConfigSettings.ExpressionExcerptLength.Value,
                pdfConfigSettings.OptionsExcerptCount.Value,
                pdfConfigSettings.MinAmountrOfDigitsInCodes.Value,
                pdfConfigSettings.AttachmentSize.Value,
                pdfConfigSettings.PdfGenerationTimeoutInSeconds.Value,
                pdfConfigSettings.VariableExpressionExcerptLength.Value);

            this.deskSettings = new DeskSettings(
                deskSettings.MultipassKey.Value, 
                deskSettings.ReturnUrlFormat.Value, 
                deskSettings.SiteKey.Value);

            this.historySettings = new QuestionnaireHistorySettings(questionnaireChangeHistoryLimit);
        }

        public void Load(IWebIocRegistry registry)
        {
            registry.BindMvcActionFilter<UnderConstructionMvcFilter>();
            registry.BindWebApiFilter<UnderConstructionHttpFilter>(/*System.Web.Http.Filters.FilterScope.Controller, 0*/);
            registry.BindMvcActionFilterWhenControllerOrActionHasNoAttribute<TransactionFilter, NoTransactionAttribute>(/*FilterScope.First,*/ 1);
            registry.BindWebApiActionFilterWhenControllerOrActionHasNoAttribute<ApiTransactionFilter, NoTransactionAttribute>(/*System.Web.Http.Filters.FilterScope.Global, 1*/);

            registry.Bind<ICommandInflater, CommandInflater>();
            registry.Bind<IQuestionnaireHelper, QuestionnaireHelper>();
            registry.Bind<IVerificationErrorsMapper, VerificationErrorsMapper>();
            registry.Bind<ISystemMailer, SystemMailer>();
            registry.Bind<IDynamicCompiler, RoslynCompiler>();
            registry.Bind<IExpressionReplacer, ExpressionReplacer>();
            registry.Bind<IMacrosSubstitutionService, MacrosSubstitutionService>();
            registry.Bind<IExpressionProcessorGenerator, QuestionnaireExpressionProcessorGenerator>();
            registry.Bind<IExpressionsGraphProvider, ExpressionsGraphProvider>();
            registry.Bind<IExpressionsPlayOrderProvider, ExpressionsPlayOrderProvider>();
            registry.Bind<IQuestionnaireInfoFactory, QuestionnaireInfoFactory>();
            registry.Bind<ICommentsService, CommentsService>();
            registry.BindToConstant<PdfSettings>(() => pdfSettings);
            registry.BindToConstant<DeskSettings>(() => deskSettings);
            registry.BindToConstant<QuestionnaireHistorySettings>(() => historySettings);
            registry.Bind<IPdfFactory, PdfFactory>();
            registry.Bind<IDeskAuthenticationService, DeskAuthenticationService>();
        }

        public Task Init(IServiceLocator serviceLocator, UnderConstructionInfo status)
        {
            return Task.CompletedTask;
        }
    }
}
