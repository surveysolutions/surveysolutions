using System;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities;
using Main.Core.Entities.SubEntities;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Implementation.Factories;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireDto;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using It = Machine.Specifications.It;
using it = Moq.It;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireDenormalizerTests
{
    internal class when_handling_TextListQuestionChanged_event_and_max_value_is_specified : QuestionnaireDenormalizerTestsContext
    {
        Establish context = () =>
        {
            var parentGroupId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            questionId = Guid.Parse("11111111111111111111111111111111");
            int maxAnswerCount = 13;
            questionnaireDocument = CreateQuestionnaireDocument(new []
            {
                CreateGroup(groupId: parentGroupId, children: new []
                {
                    CreateNumericQuestion(questionId: questionId)
                })
            });

            @event = CreateTextListQuestionChangedEvent(questionId: questionId, maxAnswerCount: maxAnswerCount);

            updatedQuestionWithMaxValue = CreateNumericQuestion(questionId, "title");

            var questionFactory = Mock.Of<IQuestionnaireEntityFactory>(factory
                => factory.CreateQuestion(it.Is<QuestionData>(data => data.PublicKey == questionId && data.MaxAnswerCount == maxAnswerCount)) == updatedQuestionWithMaxValue);

            denormalizer = CreateQuestionnaireDenormalizer(questionnaire: questionnaireDocument, questionnaireEntityFactory: questionFactory);
        };

        Because of = () =>
            denormalizer.UpdateTextListQuestion(@event);

        It should_set_question_to_question_provided_by_question_factory_for_specified_max_value = () =>
            questionnaireDocument.GetQuestion<IQuestion>(questionId).ShouldEqual(updatedQuestionWithMaxValue);

        private static Questionnaire denormalizer;
        private static TextListQuestionChanged @event;
        private static QuestionnaireDocument questionnaireDocument;
        private static Guid questionId;
        private static IQuestion updatedQuestionWithMaxValue;
    }
}