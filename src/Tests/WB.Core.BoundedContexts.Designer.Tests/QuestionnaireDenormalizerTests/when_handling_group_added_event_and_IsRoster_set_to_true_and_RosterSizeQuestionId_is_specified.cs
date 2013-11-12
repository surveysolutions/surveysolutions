using System;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Events.Questionnaire;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Document;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using It = Machine.Specifications.It;
using it = Moq.It;

namespace WB.Core.BoundedContexts.Designer.Tests.QuestionnaireDenormalizerTests
{
    internal class when_handling_group_added_event_and_IsRoster_set_to_true_and_RosterSizeQuestionId_is_specified : QuestionnaireDenormalizerTestsContext
    {
        Establish context = () =>
        {
            groupId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            rosterSizeQuestionId = Guid.Parse("11111111111111111111111111111111");

            questionnaireDocument = CreateQuestionnaireDocument();

            @event = CreateNewGroupAddedEvent(groupId: groupId, isRoster: true, rosterSizeQuestionId: rosterSizeQuestionId);

            var documentStorage = Mock.Of<IReadSideRepositoryWriter<QuestionnaireDocument>>(writer
                => writer.GetById(it.IsAny<Guid>()) == questionnaireDocument);

            denormalizer = CreateQuestionnaireDenormalizer(documentStorage: documentStorage);
        };

        Because of = () =>
            denormalizer.Handle(@event);

        It should_put_group_to_questionnaire_document = () =>
            questionnaireDocument.ShouldContainGroup(group => group.PublicKey == groupId);

        It should_set_group_IsRoster_property_to_true = () =>
            questionnaireDocument.GetGroup(groupId)
                .IsRoster.ShouldEqual(true);

        It should_set_group_RosterSizeQuestionId_property_to_specified_valued = () =>
            questionnaireDocument.GetGroup(groupId)
                .RosterSizeQuestionId.ShouldEqual(rosterSizeQuestionId);

        private static QuestionnaireDocument questionnaireDocument;
        private static Guid groupId;
        private static Guid? rosterSizeQuestionId;
        private static QuestionnaireDenormalizer denormalizer;
        private static IPublishedEvent<NewGroupAdded> @event;
    }
}