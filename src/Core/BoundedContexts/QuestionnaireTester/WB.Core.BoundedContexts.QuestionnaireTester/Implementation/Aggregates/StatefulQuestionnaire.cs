using Main.Core.Events.Questionnaire;
using Microsoft.Practices.ServiceLocation;
using WB.Core.BoundedContexts.QuestionnaireTester.Services;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Aggregates
{
    internal class StatefulQuestionnaire : Questionnaire
    {
        private static IPlainQuestionnaireRepository QuestionnaireRepository
        {
            get { return ServiceLocator.Current.GetInstance<IPlainQuestionnaireRepository>(); }
        }

        private static IQuestionnaireImportService QuestionnaireImportService
        {
            get { return ServiceLocator.Current.GetInstance<IQuestionnaireImportService>(); }
        }

        public StatefulQuestionnaire() { }

        new protected internal void Apply(TemplateImported e)
        {
            var questionnaireDocument = e.Source;
            questionnaireDocument.ConnectChildrenWithParent();

            QuestionnaireRepository.StoreQuestionnaire(questionnaireDocument.PublicKey, 1, questionnaireDocument);
            QuestionnaireImportService.ImportQuestionnaire(questionnaireDocument);
        }
    }
}
