using System;
using Machine.Specifications;
using Main.Core.Documents;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Document;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using It = Machine.Specifications.It;

namespace WB.Core.BoundedContexts.Designer.Tests.QuestionnaireDenormalizerTests
{
    internal class when_handling_RosterChanged_event : QuestionnaireDenormalizerTestsContext
    {
        Establish context = () =>
        {
            groupId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            rosterSizeQuestionId = Guid.Parse("11111111111111111111111111111111");

            questionnaireDocument = CreateQuestionnaireDocument(
                CreateGroup(groupId: groupId, setup: group => group.RosterSizeQuestionId = null)
                );

            @event = CreateRosterChangedEvent(groupId: groupId, rosterSizeQuestionId: rosterSizeQuestionId);

            var documentStorage = Mock.Of<IReadSideRepositoryWriter<QuestionnaireDocument>>(writer
                => writer.GetById(Moq.It.IsAny<Guid>()) == questionnaireDocument);

            denormalizer = CreateQuestionnaireDenormalizer(documentStorage: documentStorage);
        };

        Because of = () =>
            denormalizer.Handle(@event);

        It should_set_group_RosterSizeQuestionId_property_to_specified_valued = () =>
            questionnaireDocument.GetGroup(groupId)
                .RosterSizeQuestionId.ShouldEqual(rosterSizeQuestionId);

        private static QuestionnaireDenormalizer denormalizer;
        private static IPublishedEvent<RosterChanged> @event;
        private static QuestionnaireDocument questionnaireDocument;
        private static Guid groupId;
        private static Guid rosterSizeQuestionId;
    }
}