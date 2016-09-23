using System;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Designer.Aggregates;
using It = Machine.Specifications.It;
using it = Moq.It;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireDenormalizerTests
{
    internal class when_adding_a_group_and_there_already_is_one_group_with_same_id : QuestionnaireDenormalizerTestsContext
    {
        Establish context = () =>
        {
            var groupId = Guid.Parse("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
            addedGroupTitle = "Added Group";

            groupAddedEvent = CreateNewGroupAddedEvent(groupId: groupId, title: addedGroupTitle);

            questionnaire = CreateQuestionnaireDocument(children: new[]
            {
                existingGroup = CreateGroup(groupId: groupId, title: "Existing Group"),
            });

            denormalizer = CreateQuestionnaireDenormalizer(questionnaire: questionnaire);
        };

        Because of = () =>
            denormalizer.AddGroup(groupAddedEvent);

        It should_be_2_groups_in_total = () =>
            questionnaire.Children.Count.ShouldEqual(2);

        It should_be_existing_group_in_questionnaire = () =>
            questionnaire.Children.ShouldContain(existingGroup);

        It should_be_added_group_in_questionnaire = () =>
            questionnaire.Children.ShouldContain(group => ((Group)group).Title == addedGroupTitle);

        private static Questionnaire denormalizer;
        private static NewGroupAdded groupAddedEvent;
        private static QuestionnaireDocument questionnaire;
        private static Group existingGroup;
        private static string addedGroupTitle;
    }
}