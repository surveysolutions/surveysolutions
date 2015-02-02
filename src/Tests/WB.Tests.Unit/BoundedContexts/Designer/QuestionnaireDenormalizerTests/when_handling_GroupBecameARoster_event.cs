using System;
using Machine.Specifications;
using Main.Core.Documents;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Document;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireDenormalizerTests
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

            var documentStorage = Mock.Of<IReadSideKeyValueStorage<QuestionnaireDocument>>(writer
                => writer.GetById(Moq.It.IsAny<string>()) == questionnaireDocument);

            denormalizer = CreateQuestionnaireDenormalizer(documentStorage: documentStorage);
        };

        Because of = () =>
            denormalizer.Handle(@event);

        It should_set_group_IsRoster_property_to_true = () =>
            questionnaireDocument.GetGroup(groupId)
                .IsRoster.ShouldEqual(true);

        private static QuestionnaireDenormalizer denormalizer;
        private static IPublishedEvent<GroupBecameARoster> @event;
        private static QuestionnaireDocument questionnaireDocument;
        private static Guid groupId;
    }
}