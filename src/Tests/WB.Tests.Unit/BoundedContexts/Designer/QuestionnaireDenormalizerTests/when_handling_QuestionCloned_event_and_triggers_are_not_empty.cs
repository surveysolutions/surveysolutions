using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities.Question;
using Main.Core.Events.Questionnaire;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Designer.Implementation.Factories;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Document;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireDenormalizerTests
{
    internal class when_handling_QuestionCloned_event_and_triggers_are_not_empty : QuestionnaireDenormalizerTestsContext
    {
        Establish context = () =>
        {
            var parentGroupId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            rosterGroupId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");

            questionId = Guid.Parse("11111111111111111111111111111111");
            clonedQuestionId = Guid.Parse("22222222222222222222222222222222");

            questionnaireDocument = CreateQuestionnaireDocument(
                CreateGroup(groupId: parentGroupId, children: new List<IComposite>()
                {
                    CreateNumericQuestion(questionId),
                    CreateGroup(groupId:rosterGroupId)
                })
                );

            @event = CreateQuestionClonedEvent(clonedQuestionId, parentGroupId: parentGroupId, triggers: new List<Guid> { rosterGroupId });

            var documentStorage = Mock.Of<IReadSideKeyValueStorage<QuestionnaireDocument>>(writer
                => writer.GetById(Moq.It.IsAny<string>()) == questionnaireDocument);

            var numericQuestion = CreateNumericQuestion(clonedQuestionId, "title");
            var questionFactory = Mock.Of<IQuestionnaireEntityFactory>(factory => factory.CreateQuestion(Moq.It.IsAny<QuestionData>()) == numericQuestion);

            denormalizer = CreateQuestionnaireDenormalizer(documentStorage: documentStorage, questionnaireEntityFactory: questionFactory);
        };

        Because of = () =>
            denormalizer.Handle(@event);

        It should_add_cloned_question_with_Numeric_type = () =>
            questionnaireDocument.GetQuestion<INumericQuestion>(clonedQuestionId).ShouldNotBeNull();

        It should_contains_at_least_2_numeric_questions__original_and_clone = () =>
            questionnaireDocument.Find<INumericQuestion>(question => true).Count().ShouldEqual(2);

        It should_set_IsRoster_property_to_roster_group_from_trigger_list = () =>
            questionnaireDocument.GetGroup(rosterGroupId).IsRoster.ShouldBeTrue();

        It should_set_cloned_question_id_in_RosterSizeQuestionId_field_to_roster_group_from_trigger_list = () =>
            questionnaireDocument.GetGroup(rosterGroupId).RosterSizeQuestionId.ShouldEqual(clonedQuestionId);

        private static QuestionnaireDenormalizer denormalizer;
        private static IPublishedEvent<QuestionCloned> @event;
        private static QuestionnaireDocument questionnaireDocument;
        private static Guid questionId;
        private static Guid rosterGroupId;
        private static Guid clonedQuestionId;
    }
}