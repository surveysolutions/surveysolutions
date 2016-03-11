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

            var documentStorage = Mock.Of<IReadSideKeyValueStorage<QuestionnaireDocument>>(storage
                => storage.GetById(it.IsAny<string>()) == questionnaire);

            denormalizer = CreateQuestionnaireDenormalizer(documentStorage: documentStorage);
        };

        Because of = () =>
            denormalizer.Handle(groupDeletedEvent);

        It should_be_only_2_groups_left = () =>
            questionnaire.Children.Count.ShouldEqual(2);

        It should_result_in_only_second_and_third_groups_left = () =>
            questionnaire.Children.ShouldContainOnly(secondGroup, thirdGroup);

        private static QuestionnaireDocument questionnaire;
        private static QuestionnaireDenormalizer denormalizer;
        private static IPublishedEvent<GroupDeleted> groupDeletedEvent;
        private static Group secondGroup;
        private static Group thirdGroup;
    }
}