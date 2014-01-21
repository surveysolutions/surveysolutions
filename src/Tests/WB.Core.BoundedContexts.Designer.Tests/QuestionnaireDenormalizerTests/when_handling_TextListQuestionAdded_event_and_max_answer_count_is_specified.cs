using System;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities.Question;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Document;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using It = Machine.Specifications.It;
using it = Moq.It;

namespace WB.Core.BoundedContexts.Designer.Tests.QuestionnaireDenormalizerTests
{
    internal class when_handling_TextListQuestionAdded_event_and_max_answer_count_is_specified : QuestionnaireDenormalizerTestsContext
    {
        Establish context = () =>
        {
            var parentGroupId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            questionId = Guid.Parse("11111111111111111111111111111111");
            maxValue = 12;

            questionnaireDocument = CreateQuestionnaireDocument(
                CreateGroup(groupId: parentGroupId)
            );

            @event = CreateTextListQuestionAddedEvent(questionId: questionId, parentGroupId: parentGroupId, maxAnswerCount: maxValue);

            var documentStorage = Mock.Of<IReadSideRepositoryWriter<QuestionnaireDocument>>(writer
                => writer.GetById(it.IsAny<Guid>()) == questionnaireDocument);

            denormalizer = CreateQuestionnaireDenormalizer(documentStorage: documentStorage);
        };

        Because of = () =>
            denormalizer.Handle(@event);

        It should_set_question_MaxValue_property_to_specified_max_value = () =>
            questionnaireDocument.GetQuestion<ITextListQuestion>(questionId)
                .MaxAnswerCount.ShouldEqual(maxValue);

        private static QuestionnaireDenormalizer denormalizer;
        private static IPublishedEvent<TextListQuestionAdded> @event;
        private static QuestionnaireDocument questionnaireDocument;
        private static Guid questionId;
        private static int maxValue;
    }
}