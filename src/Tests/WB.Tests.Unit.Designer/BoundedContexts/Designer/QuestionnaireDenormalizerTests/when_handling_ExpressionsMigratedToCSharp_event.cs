using Machine.Specifications;
using Main.Core.Documents;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using It = Machine.Specifications.It;
using it = Moq.It;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireDenormalizerTests
{
    internal class when_handling_ExpressionsMigratedToCSharp_event : QuestionnaireDenormalizerTestsContext
    {
        Establish context = () =>
        {
            questionnaireDocument = CreateQuestionnaireDocument();

            @event = Create.Event.ExpressionsMigratedToCSharpEvent();

            denormalizer = CreateQuestionnaireDenormalizer(questionnaire: questionnaireDocument);
        };

        Because of = () =>
            denormalizer.MigrateExpressionsToCSharp(@event);

        It should_set_document_csharp_marker_to_true = () =>
            questionnaireDocument.UsesCSharp.ShouldEqual(true);

        private static Questionnaire denormalizer;
        private static ExpressionsMigratedToCSharp @event;
        private static QuestionnaireDocument questionnaireDocument;
    }
}