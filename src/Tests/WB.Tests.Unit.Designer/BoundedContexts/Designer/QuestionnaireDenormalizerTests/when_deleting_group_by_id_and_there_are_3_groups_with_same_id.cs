using System;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireDto;
using Group = Main.Core.Entities.SubEntities.Group;
using It = Machine.Specifications.It;
using it = Moq.It;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireDenormalizerTests
{
    internal class when_deleting_group_by_id_and_there_are_3_groups_with_same_id : QuestionnaireDenormalizerTestsContext
    {
        Establish context = () =>
        {
            var groupId = Guid.Parse("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");

            groupDeletedEvent = CreateGroupDeletedEvent(groupId);

            questionnaire = CreateQuestionnaireDocument(children: new[] 
            {
                CreateGroup(groupId: groupId, title: "Group 1"),
                secondGroup = CreateGroup(groupId: groupId, title: "Group 2"),
                thirdGroup = CreateGroup(groupId: groupId, title: "Group 3"),
            });

            denormalizer = CreateQuestionnaireDenormalizer(questionnaire: questionnaire);
        };

        Because of = () =>
            denormalizer.DeleteGroup(groupDeletedEvent);

        It should_be_only_2_groups_left = () =>
            questionnaire.Children.Count.ShouldEqual(2);

        It should_result_in_only_second_and_third_groups_left = () =>
            questionnaire.Children.ShouldContainOnly(secondGroup, thirdGroup);

        private static QuestionnaireDocument questionnaire;
        private static Questionnaire denormalizer;
        private static GroupDeleted groupDeletedEvent;
        private static Group secondGroup;
        private static Group thirdGroup;
    }
}