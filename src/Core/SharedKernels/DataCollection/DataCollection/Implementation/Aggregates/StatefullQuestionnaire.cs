using Main.Core.Documents;
using Main.Core.Events.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Events.Questionnaire;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates
{
    internal class StatefullQuestionnaire : Questionnaire
    {
        public StatefullQuestionnaire() { }

        public QuestionnaireDocument Questionnaire { get; private set; }

        protected override internal void Apply(TemplateImported e)
        {
            this.Questionnaire = e.Source;
        }

        protected override internal void Apply(QuestionnaireAssemblyImported e)
        {
        }
    }
}
