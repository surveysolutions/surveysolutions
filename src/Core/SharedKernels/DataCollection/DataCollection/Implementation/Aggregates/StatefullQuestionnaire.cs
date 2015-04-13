using Main.Core.Events.Questionnaire;
using Microsoft.Practices.ServiceLocation;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates
{
    internal class StatefullQuestionnaire : Questionnaire
    {
        private static IPlainQuestionnaireRepository QuestionnaireRepository
        {
            get { return ServiceLocator.Current.GetInstance<IPlainQuestionnaireRepository>(); }
        }

        public StatefullQuestionnaire() { }
        
        new protected internal void Apply(TemplateImported e)
        {
            QuestionnaireRepository.StoreQuestionnaire(e.Source.PublicKey, 1, e.Source);
        }
    }
}
