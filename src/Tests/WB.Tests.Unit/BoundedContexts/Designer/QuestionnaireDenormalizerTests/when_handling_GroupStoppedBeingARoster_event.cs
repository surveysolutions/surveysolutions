using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Document;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireDenormalizerTests
{
    internal class when_handling_GroupStoppedBeingARoster_event : QuestionnaireDenormalizerTestsContext
    {
        Establish context = () =>
        {
            groupId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

            questionnaireDocument = CreateQuestionnaireDocument(
                CreateGroup(groupId: groupId, setup: group =>
                {
                    group.IsRoster = true;
                    group.RosterSizeQuestionId = Guid.NewGuid();
                    group.RosterTitleQuestionId = Guid.NewGuid();
                    group.RosterSizeSource = RosterSizeSourceType.FixedTitles;
                    group.FixedRosterTitles = new Dictionary<decimal, string> {  {1, "fixed roster title"} };
                }));

            @event = CreateGroupStoppedBeingARosterEvent(groupId: groupId);

            var documentStorage = Mock.Of<IReadSideKeyValueStorage<QuestionnaireDocument>>(writer
                => writer.GetById(Moq.It.IsAny<string>()) == questionnaireDocument);

            denormalizer = CreateQuestionnaireDenormalizer(documentStorage: documentStorage);
        };

        Because of = () =>
            denormalizer.Handle(@event);

        It should_set_group_IsRoster_property_to_false = () =>
            questionnaireDocument.GetGroup(groupId)
                .IsRoster.ShouldEqual(false);

        It should_set_group_RosterSizeSource_property_to_Question = () =>
            questionnaireDocument.GetGroup(groupId)
                .RosterSizeSource.ShouldEqual(RosterSizeSourceType.Question);

        It should_set_group_RosterSizeQuestionId_property_to_null = () =>
            questionnaireDocument.GetGroup(groupId)
                .RosterSizeQuestionId.ShouldBeNull();

        It should_set_group_RosterTitleQuestionId_property_to_null = () =>
            questionnaireDocument.GetGroup(groupId)
                .RosterTitleQuestionId.ShouldBeNull();

        It should_set_group_RosterFixedTitles_property_to_empty = () =>
            questionnaireDocument.GetGroup(groupId)
                .FixedRosterTitles.Count.ShouldEqual(0);

        private static QuestionnaireDenormalizer denormalizer;
        private static IPublishedEvent<GroupStoppedBeingARoster> @event;
        private static QuestionnaireDocument questionnaireDocument;
        private static Guid groupId;
    }
}