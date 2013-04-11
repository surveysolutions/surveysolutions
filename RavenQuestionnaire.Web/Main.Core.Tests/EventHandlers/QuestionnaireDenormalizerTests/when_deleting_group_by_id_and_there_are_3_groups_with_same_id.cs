namespace Main.Core.Tests.Domain.QuestionnaireDenormalizerTests
{
    using System;

    using Machine.Specifications;

    using Main.Core.Documents;
    using Main.Core.Entities.SubEntities;
    using Main.Core.EventHandlers;
    using Main.Core.Events.Questionnaire;
    using Main.DenormalizerStorage;

    using Moq;

    using Ncqrs.Eventing.ServiceModel.Bus;

    using It = Machine.Specifications.It;
    using it = Moq.It;

    internal class when_deleting_group_by_id_and_there_are_3_groups_with_same_id : QuestionnaireDenormalizerTestsContext
    {
        Establish context = () =>
        {
            groupId = Guid.Parse("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");

            firstGroupTitle = "Group 1";
            secondGroupTitle = "Group 2";
            thirdGroupTitle = "Group 3";

            groupDeletedEvent = CreateGroupDeletedEvent(groupId);

            questionnaire = CreateQuestionnaireDocument();
            questionnaire.Children.AddRange(new []
            {
                CreateGroup(groupId: groupId, title: firstGroupTitle),
                CreateGroup(groupId: groupId, title: secondGroupTitle),
                CreateGroup(groupId: groupId, title: thirdGroupTitle),
            });

            var documentStorage = Mock.Of<IDenormalizerStorage<QuestionnaireDocument>>(storage
                => storage.GetByGuid(it.IsAny<Guid>()) == questionnaire);

            denormalizer = CreateQuestionnaireDenormalizer(documentStorage: documentStorage);
        };

        Because of = () =>
            denormalizer.Handle(groupDeletedEvent);

        It should_be_only_2_groups = () =>
            questionnaire.Children.Count.ShouldEqual(2);

        It should_remove_first_group = () =>
            questionnaire.Children.ShouldNotContain(group => ((IGroup)group).Title == firstGroupTitle);

        It should_not_remove_second_group = () =>
            questionnaire.Children.ShouldContain(group => ((IGroup)group).Title == secondGroupTitle);

        It should_not_remove_third_group = () =>
            questionnaire.Children.ShouldContain(group => ((IGroup)group).Title == thirdGroupTitle);

        private static QuestionnaireDocument questionnaire;
        private static string firstGroupTitle;
        private static string secondGroupTitle;
        private static string thirdGroupTitle;
        private static QuestionnaireDenormalizer denormalizer;
        private static Guid groupId;
        private static IPublishedEvent<GroupDeleted> groupDeletedEvent;
    }
}