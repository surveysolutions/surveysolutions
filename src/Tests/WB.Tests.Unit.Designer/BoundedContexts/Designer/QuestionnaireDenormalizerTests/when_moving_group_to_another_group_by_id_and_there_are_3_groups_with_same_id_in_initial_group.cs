using System;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Document;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using It = Machine.Specifications.It;
using it = Moq.It;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireDenormalizerTests
{
    internal class when_moving_group_to_another_group_by_id_and_there_are_3_groups_with_same_id_in_initial_group : QuestionnaireDenormalizerTestsContext
    {
        Establish context = () =>
        {
            var groupId = Guid.Parse("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
            var anotherGroupId = Guid.Parse("dadadadadadadadadadadadadadadada");

            groupMovedEvent = CreateQuestionnaireItemMovedEvent(itemId: groupId, targetGroupId: anotherGroupId);

            QuestionnaireDocument questionnaire = CreateQuestionnaireDocument(children: new[]
            {
                initialGroup = CreateGroup(children: new[] 
                {
                    firstGroup = CreateGroup(groupId: groupId, title: "Group 1"),
                    secondGroup = CreateGroup(groupId: groupId, title: "Group 2"),
                    thirdGroup = CreateGroup(groupId: groupId, title: "Group 3"),
                }),

                anotherGroup = CreateGroup(groupId: anotherGroupId),
            });

            var documentStorage = Mock.Of<IReadSideKeyValueStorage<QuestionnaireDocument>>(storage
                => storage.GetById(it.IsAny<string>()) == questionnaire);

            denormalizer = CreateQuestionnaireDenormalizer(documentStorage: documentStorage);
        };

        Because of = () =>
            denormalizer.Handle(groupMovedEvent);

        It should_move_first_group_to_another_group = () =>
            anotherGroup.Children.ShouldContainOnly(firstGroup);

        It should_leave_second_and_third_groups_in_initial_group = () =>
            initialGroup.Children.ShouldContainOnly(secondGroup, thirdGroup);

        private static Group anotherGroup;
        private static Group initialGroup;
        private static Group firstGroup;
        private static Group secondGroup;
        private static Group thirdGroup;
        private static QuestionnaireDenormalizer denormalizer;
        private static IPublishedEvent<QuestionnaireItemMoved> groupMovedEvent;
    }
}