using Main.Core.Events.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Events.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;

namespace WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Aggregates
{
    internal class StatefullQuestionnaire : Questionnaire
    {
        protected override internal void Apply(TemplateImported e)
        {
        }

        protected override internal void Apply(QuestionnaireDeleted e)
        {
        }

        protected override internal void Apply(QuestionnaireDisabled e)
        {
        }

        protected override void Apply(PlainQuestionnaireRegistered e)
        {
        }

        protected override internal void Apply(QuestionnaireAssemblyImported e)
        {
        }
    }
}
