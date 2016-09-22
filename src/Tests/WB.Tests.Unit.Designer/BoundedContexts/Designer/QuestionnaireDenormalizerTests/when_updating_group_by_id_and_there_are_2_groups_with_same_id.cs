﻿using System;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using It = Machine.Specifications.It;
using it = Moq.It;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireDenormalizerTests
{
    internal class when_updating_group_by_id_and_there_are_2_groups_with_same_id : QuestionnaireDenormalizerTestsContext
    {
        Establish context = () =>
        {
            var groupId = Guid.Parse("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");

            initialGroupTitle = "Initial Title";
            updatedGroupTitle = "Updated Title";

            groupUpdatedEvent = CreateGroupUpdatedEvent(groupId: groupId, title: updatedGroupTitle);

            QuestionnaireDocument questionnaire = CreateQuestionnaireDocument(children: new[]
            {
                firstGroup = CreateGroup(groupId: groupId, title: initialGroupTitle),
                secondGroup = CreateGroup(groupId: groupId, title: initialGroupTitle),
            });

            denormalizer = CreateQuestionnaireDenormalizer(questionnaire: questionnaire);
        };

        Because of = () =>
            denormalizer.UpdateGroup(groupUpdatedEvent);

        It should_update_first_group = () =>
            firstGroup.Title.ShouldEqual(updatedGroupTitle);

        It should_not_update_second_group = () =>
            secondGroup.Title.ShouldEqual(initialGroupTitle);

        private static Questionnaire denormalizer;
        private static GroupUpdated groupUpdatedEvent;
        private static Group firstGroup;
        private static Group secondGroup;
        private static string updatedGroupTitle;
        private static string initialGroupTitle;
    }
}