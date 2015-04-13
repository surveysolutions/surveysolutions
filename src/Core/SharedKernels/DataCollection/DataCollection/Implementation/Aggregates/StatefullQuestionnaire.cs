using Main.Core.Documents;
using Main.Core.Events.Questionnaire;

using Microsoft.Practices.ServiceLocation;

using WB.Core.SharedKernels.DataCollection.Events.Questionnaire;
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

        public QuestionnaireDocument Questionnaire { get; private set; }

        protected override internal void Apply(TemplateImported e)
        {
            QuestionnaireRepository.StoreQuestionnaire(e.Source.PublicKey, 1, e.Source);
        }

        protected override internal void Apply(QuestionnaireAssemblyImported e)
        {
        }
    }
}
