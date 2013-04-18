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
            var groupId = Guid.Parse("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");

            groupDeletedEvent = CreateGroupDeletedEvent(groupId);

            questionnaire = CreateQuestionnaireDocument();

            questionnaire.Children.AddRange(new [] 
            {
                firstGroup = CreateGroup(groupId: groupId, title: "Group 1"),
                secondGroup = CreateGroup(groupId: groupId, title: "Group 2"),
                thirdGroup = CreateGroup(groupId: groupId, title: "Group 3"),
            });

            var documentStorage = Mock.Of<IDenormalizerStorage<QuestionnaireDocument>>(storage
                => storage.GetByGuid(it.IsAny<Guid>()) == questionnaire);

            denormalizer = CreateQuestionnaireDenormalizer(documentStorage: documentStorage);
        };

        Because of = () =>
            denormalizer.Handle(groupDeletedEvent);

        It should_be_only_2_groups_left = () =>
            questionnaire.Children.Count.ShouldEqual(2);

        It should_remove_first_group = () =>
            questionnaire.Children.ShouldNotContain(firstGroup);

        It should_not_remove_second_group = () =>
            questionnaire.Children.ShouldContain(secondGroup);

        It should_not_remove_third_group = () =>
            questionnaire.Children.ShouldContain(thirdGroup);

        private static QuestionnaireDocument questionnaire;
        private static QuestionnaireDenormalizer denormalizer;
        private static IPublishedEvent<GroupDeleted> groupDeletedEvent;
        private static Group firstGroup;
        private static Group secondGroup;
        private static Group thirdGroup;
    }
}