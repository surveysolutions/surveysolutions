using System;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireDto;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireDenormalizerTests
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
                    group.FixedRosterTitles = new[] { new FixedRosterTitle(1, "fixed roster title") };
                }));

            @event = CreateGroupStoppedBeingARosterEvent(groupId: groupId);

            denormalizer = CreateQuestionnaireDenormalizer(questionnaire: questionnaireDocument);
        };

        Because of = () =>
            denormalizer.RemoveRosterFlagFromGroup(@event);

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
                .FixedRosterTitles.Length.ShouldEqual(0);

        private static Questionnaire denormalizer;
        private static GroupStoppedBeingARoster @event;
        private static QuestionnaireDocument questionnaireDocument;
        private static Guid groupId;
    }
}