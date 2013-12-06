using Main.Core.Utility;
using Ninject.Modules;
using WB.Core.BoundedContexts.Designer.Implementation.Factories;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Account;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Document;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Pdf;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.SharedPersons;
using WB.Core.Infrastructure.FunctionalDenormalization;

namespace WB.Core.BoundedContexts.Designer
{
    public class DesignerBoundedContextModule : NinjectModule
    {
        public override void Load()
        {
            this.Bind<IJsonExportService>().To<JsonExportService>().InSingletonScope();
            this.Bind<IQuestionnaireDocumentUpgrader>().To<QuestionnaireDocumentUpgrader>().InSingletonScope();
            this.Bind<IQuestionFactory>().To<QuestionFactory>().InSingletonScope();

            DispatcherRegistryHelper.RegisterDenormalizer<AccountDenormalizer>(this.Kernel);
            DispatcherRegistryHelper.RegisterDenormalizer<QuestionnaireDenormalizer>(this.Kernel);
            DispatcherRegistryHelper.RegisterDenormalizer<QuestionnaireSharedPersonsDenormalizer>(this.Kernel);
            DispatcherRegistryHelper.RegisterDenormalizer<QuestionnaireListViewItemDenormalizer>(this.Kernel);
            DispatcherRegistryHelper.RegisterDenormalizer<PdfQuestionnaireDenormalizer>(this.Kernel);

            RegistryHelper.RegisterFactory<QuestionnaireListViewFactory>(this.Kernel);
            RegistryHelper.RegisterFactory<QuestionnaireViewFactory>(this.Kernel);
            RegistryHelper.RegisterFactory<QuestionnaireSharedPersonsFactory>(this.Kernel);
            RegistryHelper.RegisterFactory<AccountListViewFactory>(this.Kernel);
            RegistryHelper.RegisterFactory<AccountViewFactory>(this.Kernel);
            RegistryHelper.RegisterFactory<PdfQuestionnaireFactory>(this.Kernel);
        }
    }
}
