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

            var documentStorage = Mock.Of<IReadSideKeyValueStorage<QuestionnaireDocument>>(storage
                => storage.GetById(it.IsAny<string>()) == questionnaire);

            denormalizer = CreateQuestionnaireDenormalizer(documentStorage: documentStorage);
        };

        Because of = () =>
            denormalizer.Handle(groupAddedEvent);

        It should_be_2_groups_in_total = () =>
            questionnaire.Children.Count.ShouldEqual(2);

        It should_be_existing_group_in_questionnaire = () =>
            questionnaire.Children.ShouldContain(existingGroup);

        It should_be_added_group_in_questionnaire = () =>
            questionnaire.Children.ShouldContain(group => ((Group)group).Title == addedGroupTitle);

        private static QuestionnaireDenormalizer denormalizer;
        private static IPublishedEvent<NewGroupAdded> groupAddedEvent;
        private static QuestionnaireDocument questionnaire;
        private static Group existingGroup;
        private static string addedGroupTitle;
    }
}