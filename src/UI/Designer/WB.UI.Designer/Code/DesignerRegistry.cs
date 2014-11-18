﻿using Ncqrs;
using Ninject.Modules;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Services.CodeGeneration;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.ChapterInfo;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionnaireInfo;
using WB.Core.Infrastructure.CommandBus;
using WB.UI.Designer.Code.Implementation;
using WB.UI.Designer.Mailers;
using WB.UI.Designer.WebServices;

namespace WB.UI.Designer.Code
{
    public class DesignerRegistry : NinjectModule
    {
        public override void Load()
        {
            ICommandListSupplier commands = new CommandListSupplier(CommandRegistry.GetRegisteredCommands());
            this.Bind<ICommandListSupplier>().ToConstant(commands);

            this.Bind<ICommandPreprocessor>().To<CommandPreprocessor>();
            this.Bind<IQuestionnaireHelper>().To<QuestionnaireHelper>();
            this.Bind<IVerificationErrorsMapper>().To<VerificationErrorsMapper>();
            this.Bind<ISystemMailer>().To<SystemMailer>();
            this.Bind<IPublicService>().To<PublicService>();
            this.Bind<IDynamicCompiler>().To<RoslynCompiler>();
            this.Bind<ICodeGenerator>().To<CodeGenerator>();
            this.Bind<IExpressionReplacer>().To<ExpressionReplacer>();
            this.Bind<IExpressionProcessorGenerator>().To<QuestionnireExpressionProcessorGenerator>();
            this.Bind<IChapterInfoViewFactory>().To<ChapterInfoViewFactory>();
            this.Bind<IQuestionnaireInfoFactory>().To<QuestionnaireInfoFactory>();
            this.Bind<IQuestionnaireInfoViewFactory>().To<QuestionnaireInfoViewFactory>();
        }
    }
}