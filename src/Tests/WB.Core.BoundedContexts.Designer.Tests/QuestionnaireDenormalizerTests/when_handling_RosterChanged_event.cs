using System;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Document;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using It = Machine.Specifications.It;
using it = Moq.It;

namespace WB.Core.BoundedContexts.Designer.Tests.QuestionnaireDenormalizerTests
{
    internal class when_handling_RosterChanged_event : QuestionnaireDenormalizerTestsContext
    {
        Establish context = () =>
        {
            groupId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            rosterSizeQuestionId = Guid.Parse("11111111111111111111111111111111");
            rosterTitleQuestionId = Guid.Parse("22222222222222222222222222222222");
            rosterSizeSource = RosterSizeSourceType.FixedTitles;
            rosterFixedTitles = new[] { rosterFixedTitle1, rosterFixedTitle2 };

            questionnaireDocument = CreateQuestionnaireDocument(
                CreateGroup(groupId: groupId, setup: group =>
                {
                    group.RosterSizeQuestionId = null;
                    group.RosterSizeSource = RosterSizeSourceType.Question;
                })
            );

            @event = CreateRosterChangedEvent(groupId: groupId, rosterSizeQuestionId: rosterSizeQuestionId,
                rosterSizeSource: rosterSizeSource, rosterFixedTitles: rosterFixedTitles, rosterTitleQuestionId: rosterTitleQuestionId);

            var documentStorage = Mock.Of<IReadSideRepositoryWriter<QuestionnaireDocument>>(writer
                => writer.GetById(it.IsAny<Guid>()) == questionnaireDocument);

            denormalizer = CreateQuestionnaireDenormalizer(documentStorage: documentStorage);
        };

        Because of = () =>
            denormalizer.Handle(@event);

        It should_set_group_RosterSizeQuestionId_property_to_specified_value = () =>
            questionnaireDocument.GetGroup(groupId)
                .RosterSizeQuestionId.ShouldEqual(rosterSizeQuestionId);

        It should_set_group_RosterSizeSource_property_to_specified_value = () =>
            questionnaireDocument.GetGroup(groupId)
                .RosterSizeSource.ShouldEqual(rosterSizeSource);

        It should_set_group_RosterFixedTitles_property_to_2_items_array = () =>
            questionnaireDocument.GetGroup(groupId)
                .RosterFixedTitles.Length.ShouldEqual(rosterFixedTitles.Length);

        It should_set_first_item_of_groups_RosterFixedTitles_property_to_rosterFixedTitle1_value = () =>
            questionnaireDocument.GetGroup(groupId)
                .RosterFixedTitles[0].ShouldEqual(rosterFixedTitle1);

        It should_set_second_item_of_groups_RosterFixedTitles_property_to_rosterFixedTitle2_value = () =>
            questionnaireDocument.GetGroup(groupId)
                .RosterFixedTitles[1].ShouldEqual(rosterFixedTitle2);

        It should_set_group_RosterTitleQuestionId_property_to_specified_value = () =>
            questionnaireDocument.GetGroup(groupId)
                .RosterTitleQuestionId.ShouldEqual(rosterTitleQuestionId);


        private static QuestionnaireDenormalizer denormalizer;
        private static IPublishedEvent<RosterChanged> @event;
        private static QuestionnaireDocument questionnaireDocument;
        private static Guid groupId;
        private static Guid rosterSizeQuestionId;
        private static RosterSizeSourceType rosterSizeSource;
        private static string[] rosterFixedTitles;
        private static string rosterFixedTitle1 = "title1";
        private static string rosterFixedTitle2 = "title2";
        private static Guid rosterTitleQuestionId;
    }
}