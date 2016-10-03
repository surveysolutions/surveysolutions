using System;
using Machine.Specifications;
using Main.Core.Documents;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireDto;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireDenormalizerTests
{
    internal class when_handling_GroupBecameARoster_event : QuestionnaireDenormalizerTestsContext
    {
        Establish context = () =>
        {
            groupId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

            questionnaireDocument = CreateQuestionnaireDocument(
                CreateGroup(groupId: groupId, setup: group => group.IsRoster = false)
                );

            @event = CreateGroupBecameARosterEvent(groupId: groupId);

            denormalizer = CreateQuestionnaireDenormalizer(questionnaire: questionnaireDocument);
        };

        Because of = () =>
            denormalizer.MarkGroupAsRoster(@event);

        It should_set_group_IsRoster_property_to_true = () =>
            questionnaireDocument.GetGroup(groupId)
                .IsRoster.ShouldEqual(true);

        private static Questionnaire denormalizer;
        private static GroupBecameARoster @event;
        private static QuestionnaireDocument questionnaireDocument;
        private static Guid groupId;
    }
}