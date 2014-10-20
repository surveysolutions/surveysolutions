using System;
using Machine.Specifications;
using Main.Core.Documents;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Document;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using It = Machine.Specifications.It;
using it = Moq.It;

namespace WB.Core.BoundedContexts.Designer.Tests.QuestionnaireDenormalizerTests
{
    internal class when_handling_ExpressionsMigratedToCSharp_event : QuestionnaireDenormalizerTestsContext
    {
        Establish context = () =>
        {
            questionnaireDocument = CreateQuestionnaireDocument();

            @event = Create.Event.ExpressionsMigratedToCSharpEvent().ToPublishedEvent();

            var documentStorage = Mock.Of<IReadSideRepositoryWriter<QuestionnaireDocument>>(writer
                => writer.GetById(it.IsAny<string>()) == questionnaireDocument);

            denormalizer = CreateQuestionnaireDenormalizer(documentStorage: documentStorage);
        };

        Because of = () =>
            denormalizer.Handle(@event);

        It should_set_document_language_marker_to_csharp = () =>
            questionnaireDocument.AreExpressionsInCSharpLanguage.ShouldBeTrue();

        private static QuestionnaireDenormalizer denormalizer;
        private static IPublishedEvent<ExpressionsMigratedToCSharp> @event;
        private static QuestionnaireDocument questionnaireDocument;
    }
}