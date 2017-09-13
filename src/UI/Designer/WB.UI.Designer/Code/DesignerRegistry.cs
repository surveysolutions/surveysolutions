﻿using System.Linq;
using System.Web.Mvc;
using Ninject.Modules;
using Ninject.Web.Mvc.FilterBindingSyntax;
using Ninject.Web.WebApi.FilterBindingSyntax;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Services.CodeGeneration;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Pdf;
using WB.UI.Designer.Code.ConfigurationManager;
using WB.UI.Designer.Code.Implementation;
using WB.UI.Designer.Implementation.Services;
using WB.UI.Designer.Mailers;
using WB.UI.Designer.Services;
using WB.UI.Shared.Web.Attributes;
using WB.UI.Shared.Web.Filters;

namespace WB.UI.Designer.Code
{
    public class DesignerRegistry : NinjectModule
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

        public override void Load()
        {
            this.BindFilter<PlainTransactionFilter>(FilterScope.First, 0)
                .WhenActionMethodHasNo<NoTransactionAttribute>();
            this.BindHttpFilter<PlainApiTransactionFilter>(System.Web.Http.Filters.FilterScope.Global)
                .When((controllerContext, actionDescriptor) => !actionDescriptor.GetCustomAttributes(typeof(NoTransactionAttribute)).Any());

            this.Bind<ICommandInflater>().To<CommandInflater>();
            this.Bind<IQuestionnaireHelper>().To<QuestionnaireHelper>();
            this.Bind<IVerificationErrorsMapper>().To<VerificationErrorsMapper>();
            this.Bind<ISystemMailer>().To<SystemMailer>();
            this.Bind<IDynamicCompiler>().To<RoslynCompiler>();
            this.Bind<IExpressionReplacer>().To<ExpressionReplacer>();
            this.Bind<IMacrosSubstitutionService>().To<MacrosSubstitutionService>();
            this.Bind<IExpressionProcessorGenerator>().To<QuestionnaireExpressionProcessorGenerator>();
            this.Bind<IExpressionsPlayOrderProvider>().To<ExpressionsPlayOrderProvider>();
            this.Bind<IQuestionnaireInfoFactory>().To<QuestionnaireInfoFactory>();
            this.Bind<PdfSettings>().ToConstant(pdfSettings);
            this.Bind<DeskSettings>().ToConstant(deskSettings);
            this.Bind<QuestionnaireHistorySettings>().ToConstant(historySettings);
            this.Bind<IPdfFactory>().To<PdfFactory>();
            this.Bind<IDeskAuthenticationService>().To<DeskAuthenticationService>();
        }
    }
}